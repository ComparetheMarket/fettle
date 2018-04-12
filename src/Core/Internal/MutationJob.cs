using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{
    internal class MutationJob
    {
        private readonly SyntaxNode originalClassRoot;
        private readonly string methodName;
        private readonly Config config;
        private readonly IMutator mutator;
        private readonly ICoverageAnalysisResult coverageAnalysisResult;
        
        public SyntaxNode MutatedClassRoot { get; private set; }
        public SyntaxNode OriginalNode { get; }
        public Document OriginalClass { get;  }
        
        public MutationJob(
            SyntaxNode originalClassRoot,
            SyntaxNode originalNode,
            Document originalClass,
            string methodName,
            Config config,
            IMutator mutator,
            ICoverageAnalysisResult coverageAnalysisResult)
        {
            OriginalNode = originalNode;
            OriginalClass = originalClass;

            this.originalClassRoot = originalClassRoot;
            this.methodName = methodName;
            this.config = config;
            this.mutator = mutator;
            this.coverageAnalysisResult = coverageAnalysisResult;
        }

        public async Task<SurvivingMutant> Run(ITestRunner testRunner, string tempDirectory, IEventListener eventListener)
        {
            var mutatedNode = mutator.Mutate(OriginalNode);
            MutatedClassRoot = originalClassRoot.ReplaceNode(OriginalNode, mutatedNode);

            var compilationResult = await CompileContainingProject(tempDirectory);
            if (!compilationResult.Success)
            {
                // Not all mutations are valid in all circumstances, and therefore may not compile.
                // E.g. "a + b" => "a - b" works when a and b are integers but not when they're strings.
                return null;
            }

            CopyMutatedAssemblyIntoTempTestAssemblyDirectories(compilationResult.OutputFilePath, tempDirectory, config);
            var copiedTempTestAssemblyFilePaths = TempTestAssemblyFilePaths(config, tempDirectory).ToArray();

            var ranAnyTests = false;

            for (var testAssemblyIndex = 0; testAssemblyIndex < config.TestAssemblyFilePaths.Length; ++testAssemblyIndex)
            {
                var originalTestAssemblyFilePath = config.TestAssemblyFilePaths[testAssemblyIndex];
                var tempTestAssemblyFilePath = copiedTempTestAssemblyFilePaths[testAssemblyIndex];

                var testsToRun = coverageAnalysisResult.TestsThatCoverMethod(methodName, originalTestAssemblyFilePath);
                if (testsToRun.Any())
                {
                    ranAnyTests = true;

                    var result = testRunner.RunTests(new[] {tempTestAssemblyFilePath}, testsToRun);
                    if (result.Status == TestRunStatus.SomeTestsFailed)
                    {
                        return null;
                    }
                }
            }

            return ranAnyTests ? await SurvivingMutant.CreateFrom(this) : null;
        }

        private async Task<(bool Success, string OutputFilePath)> CompileContainingProject(string outputDirectory)
        {
            var project = OriginalClass.Project;

            var compilation = (await project.GetCompilationAsync().ConfigureAwait(false))
                .RemoveSyntaxTrees(await OriginalClass.GetSyntaxTreeAsync().ConfigureAwait(false))
                .AddSyntaxTrees(MutatedClassRoot.SyntaxTree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var mutatedAssemblyFilePath = Path.Combine(outputDirectory, $"{project.AssemblyName}.dll");

            var result = ProjectCompilation.CompileProject(mutatedAssemblyFilePath, compilation);
            return result.Success ?
                (true, mutatedAssemblyFilePath) :
                (false, null);
        }

        private static void CopyMutatedAssemblyIntoTempTestAssemblyDirectories(
            string mutatedAssemblyFilePath,
            string tempDirectory,
            Config config)
        {
            foreach (var originalTestAssemblyFilePath in config.TestAssemblyFilePaths)
            {
                var dir = Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension(originalTestAssemblyFilePath));
                File.Copy(mutatedAssemblyFilePath, Path.Combine(dir, Path.GetFileName(mutatedAssemblyFilePath)),
                    overwrite: true);                
            }
        }

        private static IEnumerable<string> TempTestAssemblyFilePaths(Config config, string tempDirectory)
        {
            foreach (var originalTestAssemblyFilePath in config.TestAssemblyFilePaths)
            {
                var dir = Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension(originalTestAssemblyFilePath));

                var testAssemblyFilePath = Path.Combine(dir, Path.GetFileName(originalTestAssemblyFilePath));
                yield return testAssemblyFilePath;
            }
        }
    }
}