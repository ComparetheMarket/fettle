using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Fettle.Core
{
    public class CoverageAnalyser : ICoverageAnalyser
    {
        private readonly IEventListener eventListener;
        private readonly ITestFinder testFinder;
        private readonly ITestRunner testRunner;

        public const string CoverageOutputLinePrefix = "fettle_covered_method:";

        public CoverageAnalyser(IEventListener eventListener) : 
            this(eventListener, new NUnitTestEngine(), new NUnitTestEngine())
        {
        }

        internal CoverageAnalyser(IEventListener eventListener, ITestFinder testFinder, ITestRunner testRunner)
        {
            this.eventListener = eventListener;
            this.testFinder = testFinder;
            this.testRunner = testRunner;
        }

        public async Task<ICoverageAnalysisResult> AnalyseMethodCoverage(Config config)
        {
            var methodIdsToNames = new Dictionary<string, string>();
            
            var baseTempDirectory = TempDirectory.Create();

            try
            {
                var copiedTestAssemblyFilePaths = 
                    CopyTestAssembliesToTempDirectories(
                        config.TestAssemblyFilePaths, 
                        baseTempDirectory)
                    .ToList();

                using (var workspace = MSBuildWorkspaceFactory.Create())
                {
                    var solution = await workspace.OpenSolutionAsync(config.SolutionFilePath);

                    await InstrumentThenCompileMultipleProjects(
                        solution.Projects.Where(p => Filtering.ShouldMutateProject(p, config)),
                        config,
                        baseTempDirectory,
                        copiedTestAssemblyFilePaths,
                        methodIdsToNames);

                    var result = new CoverageAnalysisResult();

                    for (int testAssemblyIndex = 0; testAssemblyIndex < config.TestAssemblyFilePaths.Length; ++testAssemblyIndex)
                    {
                        var copiedTestAssemblyFilePath = copiedTestAssemblyFilePaths[testAssemblyIndex];

                        var tests = testFinder.FindTests(new [] { copiedTestAssemblyFilePath });

                        var runResult = testRunner.RunTestsAndAnalyseCoverage(
                            testAssemblyFilePaths: new [] { copiedTestAssemblyFilePath }, 
                            testMethodNames: tests, 
                            methodIdsToNames: methodIdsToNames, 
                            onAnalysingTestCase: (test, index) => eventListener.BeginCoverageAnalysisOfTestCase(test, index, tests.Length));

                        if (runResult.Status != TestRunStatus.AllTestsPassed)
                        {
                            return CoverageAnalysisResult.Error(runResult.Error);
                        }

                        var originalTestAssemblyFilePath = config.TestAssemblyFilePaths[testAssemblyIndex];
                        result = result.WithCoveredMethods(runResult.MethodsAndCoveringTests, originalTestAssemblyFilePath);
                    }

                    return result;
                }
            }
            finally
            {
                Directory.Delete(baseTempDirectory, recursive: true);
            }
        }
        
        private static async Task InstrumentThenCompileMultipleProjects(
            IEnumerable<Project> projects,
            Config config,
            string baseTempDirectory,
            IList<string> copiedTestAssemblyFilePaths,
            IDictionary<string, string> methodIdsToNames)
        {
            foreach (var project in projects)
            {
                var outputFilePath = Path.Combine(baseTempDirectory, $@"{project.AssemblyName}.dll");

                await InstrumentThenCompileProject(project, config, outputFilePath, methodIdsToNames);

                CopyInstrumentedAssemblyIntoTempTestAssemblyDirectories(
                    outputFilePath, 
                    copiedTestAssemblyFilePaths.Select(Path.GetDirectoryName));
            }
        }

        private static async Task InstrumentThenCompileProject(
            Project project,
            Config config,
            string outputFilePath,
            IDictionary<string, string> methodIdsToNames)
        {
            var originalSyntaxTrees = new List<SyntaxTree>();
            var modifiedSyntaxTrees = new List<SyntaxTree>();

            var classesToInstrument = project.Documents
                .Where(d => Filtering.ShouldMutateClass(d, config))
                .Where(d => !d.IsAutomaticallyGenerated());

            foreach (var originalClass in classesToInstrument)
            {
                var originalSyntaxTree = await originalClass.GetSyntaxTreeAsync().ConfigureAwait(false);
                originalSyntaxTrees.Add(originalSyntaxTree);
                modifiedSyntaxTrees.Add(
                    await InstrumentDocument(originalSyntaxTree, originalClass, methodIdsToNames));
            }

            var compilation = (await project.GetCompilationAsync().ConfigureAwait(false))
                .RemoveSyntaxTrees(originalSyntaxTrees)
                .AddSyntaxTrees(modifiedSyntaxTrees);

            var compilationResult = ProjectCompilation.CompileProject(
                outputFilePath,
                compilation);
            if (!compilationResult.Success)
            {
                var diagnostics = string.Join(Environment.NewLine, compilationResult.Diagnostics);
                throw new Exception(
                    $"Failed to compile project {compilation.AssemblyName}{Environment.NewLine}{diagnostics}");
            }
        }

        private static async Task<SyntaxTree> InstrumentDocument(
            SyntaxTree originalSyntaxTree,
            Document document,
            IDictionary<string,string> methodIdsToNames)
        {
            var root = await originalSyntaxTree.GetRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var documentEditor = DocumentEditor.CreateAsync(document).Result;

            foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                foreach (var methodNode in classNode.DescendantNodes()
                                                    .OfType<MethodDeclarationSyntax>()
                                                    .Where(methodNode => methodNode.CanInstrument()))
                {
                    var fullMethodName = methodNode.ChildNodes().First().NameOfContainingMethod(semanticModel);
                    var methodId = Guid.NewGuid().ToString();
                    methodIdsToNames.Add(methodId, fullMethodName);

                    InstrumentMethod(methodId, methodNode, documentEditor);
                }
            }

            return await documentEditor.GetChangedDocument().GetSyntaxTreeAsync();
        }

        private static void InstrumentMethod(string methodId, MethodDeclarationSyntax methodNode, DocumentEditor documentEditor)
        {
            var instrumentationNode = SyntaxFactory.ParseStatement(
                $"System.Console.WriteLine(\"{CoverageOutputLinePrefix}{methodId}\");");

            var isMethodExpressionBodied = methodNode.ExpressionBody != null;

            if (isMethodExpressionBodied)
            {
                // Replace expression body (which can only have one statement) with a normal method body
                // so that we can add the extra instrumentation statement.
                var newMethodNode = methodNode
                    .WithExpressionBody(null)
                    .WithBody(
                        SyntaxFactory.Block(
                            instrumentationNode,
                            SyntaxFactory.ReturnStatement(methodNode.ExpressionBody.Expression)));

                documentEditor.ReplaceNode(methodNode, newMethodNode);
            }
            else
            {
                var firstChildNode = methodNode.Body.ChildNodes().FirstOrDefault();
                var isMethodEmpty = firstChildNode == null;
                if (isMethodEmpty)
                {
                    documentEditor.ReplaceNode(
                        methodNode,
                        methodNode.WithBody(SyntaxFactory.Block(instrumentationNode)));
                }
                else
                {
                    documentEditor.InsertBefore(firstChildNode, instrumentationNode);
                }
            }
        }

        private static void CopyInstrumentedAssemblyIntoTempTestAssemblyDirectories(
            string instrumentedAssemblyFilePath,
            IEnumerable<string> copiedTestAssemblyDirectories)
        {
            foreach (var copiedTestAssemblyDirectory in copiedTestAssemblyDirectories)
            {
                File.Copy(
                    instrumentedAssemblyFilePath, 
                    Path.Combine(copiedTestAssemblyDirectory, Path.GetFileName(instrumentedAssemblyFilePath)),
                    overwrite: true);
            }
        }

        private static IEnumerable<string> CopyTestAssembliesToTempDirectories(
            IEnumerable<string> testAssemblyFilePaths,
            string baseTempDirectory)
        {
            foreach (var testAssemblyFilePath in testAssemblyFilePaths)
            {
                var fromDir = Path.GetDirectoryName(testAssemblyFilePath);
                var toDir = Path.Combine(baseTempDirectory, Path.GetFileNameWithoutExtension(testAssemblyFilePath));
                Directory.CreateDirectory(toDir);
                DirectoryUtils.CopyDirectoryContents(fromDir, toDir);

                yield return Path.Combine(toDir, Path.GetFileName(testAssemblyFilePath));
            }
        }
    }
}
