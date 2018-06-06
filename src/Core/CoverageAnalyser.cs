using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
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
            var methodIdsToNames = new Dictionary<long, string>();
            
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
                    var projects = solution.Projects.Where(p => Filtering.ShouldMutateProject(p, config));

                    await InstrumentThenCompileMultipleProjects(
                        projects,
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
            IDictionary<long, string> methodIdsToNames)
        {
            foreach (var project in projects)
            {
                var outputFilePath = Path.Combine(baseTempDirectory, $@"{project.AssemblyName}.dll");
                
                InstrumentThenCompileProject(project, config, outputFilePath, methodIdsToNames);

                CopyInstrumentedAssemblyIntoTempTestAssemblyDirectories(
                    outputFilePath, 
                    copiedTestAssemblyFilePaths.Select(Path.GetDirectoryName));
            }
        }

        private static void InstrumentThenCompileProject(
            Project project,
            Config config,
            string outputFilePath,
            IDictionary<long, string> methodIdsToNames)
        {
            var originalSyntaxTrees = new List<SyntaxTree>();
            var modifiedSyntaxTrees = new List<SyntaxTree>();

            var classesToInstrument = project.Documents
                .Where(d => Filtering.ShouldMutateClass(d, config))
                .Where(d => !d.IsAutomaticallyGenerated());
            
            foreach (var originalClass in classesToInstrument)
            {
                var originalSyntaxTree = originalClass.GetSyntaxTreeAsync().Result;
                originalSyntaxTrees.Add(originalSyntaxTree);
                modifiedSyntaxTrees.Add(
                    InstrumentDocument(originalSyntaxTree, originalClass, methodIdsToNames));
            }

            var compilation = (project.GetCompilationAsync().Result)
                .RemoveSyntaxTrees(originalSyntaxTrees)
                .AddSyntaxTrees(modifiedSyntaxTrees)
                .AddSyntaxTrees(CreateCollectorClass());

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

        public static class Collector
        {
            public static void MethodCalled(long methodId, string fullMethodName)
            {
                using (var mutex = new System.Threading.Mutex(false, "Global\\fettle_coverage_741CBFBB-EEB3-4E4B-88F9-E885F0EE8ADA"))
                {
                    try
                    {
                        mutex.WaitOne();

                        using (var file = MemoryMappedFile.OpenExisting("fettle_coverage"))
                        //using (var accessor = file.CreateViewAccessor(methodId*4, 4))
                        //{
                        //    accessor.Write(0, (byte)0xFF);
                        //    Console.WriteLine($"Fettle method exec: {fullMethodName}");
                        //}
                        using (var accessor = file.CreateViewStream(methodId, 0))
                        {
                            accessor.WriteByte(0xFF);
                        }
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        private static SyntaxTree CreateCollectorClass()
        {
            return SyntaxFactory.ParseSyntaxTree($@"
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace ___FettleAutoGenerated
{{
    internal static class Collector
    {{
        public static void MethodCalled(long methodId, string fullMethodName)
        {{
            using (var mutex = new System.Threading.Mutex(false, ""Global\\fettle_coverage_741CBFBB-EEB3-4E4B-88F9-E885F0EE8ADA""))
            {{
                try
                {{
                    mutex.WaitOne();

                    using (var file = MemoryMappedFile.OpenExisting(""fettle_coverage""))
                    using (var accessor = file.CreateViewAccessor(methodId, 0))
                    {{
                        accessor.Write(0, (byte)0xAA);
                        Console.WriteLine($""Fettle method exec: {{fullMethodName}} ({{methodId}})"");
                    }}
                }}
                finally
                {{
                    mutex.ReleaseMutex();
                }}
            }}
        }}
    }}
}}"
                );
        }

        private static SyntaxTree InstrumentDocument(
            SyntaxTree originalSyntaxTree,
            Document document,
            IDictionary<long,string> methodIdsToNames)
        {
            var root = originalSyntaxTree.GetRootAsync().Result;
            var semanticModel = document.GetSemanticModelAsync().Result;
            var documentEditor = DocumentEditor.CreateAsync(document).Result;

            foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                foreach (var methodNode in classNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode);
                    if (!methodSymbol.IsAbstract)
                    {
                        var fullMethodName = methodNode.ChildNodes().First().NameOfContainingMethod(semanticModel);
                        var methodId = methodIdsToNames.LongCount();
                        methodIdsToNames.Add(methodId, fullMethodName);
                        
                        InstrumentMethod(methodNode, methodId, fullMethodName, documentEditor);
                    }
                }
            }

            return documentEditor.GetChangedDocument().GetSyntaxTreeAsync().Result;
        }

        private static void InstrumentMethod(MethodDeclarationSyntax methodNode, long methodId, string fullMethodName, DocumentEditor documentEditor)
        {
            var instrumentationNode = SyntaxFactory.ParseStatement(
                $@"___FettleAutoGenerated.Collector.MethodCalled({methodId}, ""{fullMethodName}"");");

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
