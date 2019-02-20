using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{
    internal class MutationJob
    {
        private readonly SyntaxNode originalSyntaxRoot;
        private readonly string memberName;
        private readonly Config config;
        private readonly IMutator mutator;
        private readonly ICoverageAnalysisResult coverageAnalysisResult;
        
        public SyntaxNode MutatedSyntaxRoot { get; private set; }
        public SyntaxNode OriginalNode { get; }
        public Document OriginalClass { get;  }
        
        public MutationJob(
            SyntaxNode originalSyntaxRoot,
            SyntaxNode originalNode,
            Document originalClass,
            string memberName,
            Config config,
            IMutator mutator,
            ICoverageAnalysisResult coverageAnalysisResult)
        {
            OriginalNode = originalNode;
            OriginalClass = originalClass;

            this.originalSyntaxRoot = originalSyntaxRoot;
            this.memberName = memberName;
            this.config = config;
            this.mutator = mutator;
            this.coverageAnalysisResult = coverageAnalysisResult;
        }

        public async Task<(MutantStatus, Mutant)> Run(ITestRunner testRunner, string tempDirectory, IEventListener eventListener)
        {
            var mutatedNode = mutator.Mutate(OriginalNode);
            MutatedSyntaxRoot = originalSyntaxRoot.ReplaceNode(OriginalNode, mutatedNode);

            var mutant = await Mutant.Create(OriginalClass, OriginalNode, MutatedSyntaxRoot);

            var compilationResult = await CompileContainingProject(tempDirectory);
            if (!compilationResult.Success)
            {
                // Not all mutations are valid in all circumstances, and therefore may not compile.
                // E.g. "a + b" => "a - b" works when a and b are integers but not when they're strings.
                eventListener.MutantSkipped(mutant, "mutation resulted in invalid code");
                return (MutantStatus.Skipped, mutant);
            }

            CopyMutatedAssemblyIntoTempTestAssemblyDirectories(compilationResult.OutputFilePath, tempDirectory, config);
            var copiedTempTestAssemblyFilePaths = TempTestAssemblyFilePaths(config, tempDirectory).ToArray();

            var ranAnyTests = false;

            for (var testAssemblyIndex = 0; testAssemblyIndex < config.TestAssemblyFilePaths.Length; ++testAssemblyIndex)
            {
                var originalTestAssemblyFilePath = config.TestAssemblyFilePaths[testAssemblyIndex];
                var tempTestAssemblyFilePath = copiedTempTestAssemblyFilePaths[testAssemblyIndex];

                string[] testsToRun = null;
                if (coverageAnalysisResult != null)
                {
                    testsToRun = coverageAnalysisResult.TestsThatCoverMember(memberName, originalTestAssemblyFilePath);
                    if (!testsToRun.Any())
                    {
                        continue;
                    }
                }

                var result = testsToRun != null ?
                    testRunner.RunTests(new[] {tempTestAssemblyFilePath}, testsToRun) :
                    testRunner.RunAllTests(new[] {tempTestAssemblyFilePath});

                ranAnyTests = true;

                if (result.Status == TestRunStatus.SomeTestsFailed)
                {
                    eventListener.MutantKilled(mutant);
                    return (MutantStatus.Dead, mutant);
                }
            }

            if (!ranAnyTests)
            {
                eventListener.MutantSkipped(mutant, "no covering tests");
                return (MutantStatus.Skipped, mutant);
            }

            eventListener.MutantSurvived(mutant);
            return (MutantStatus.Alive, mutant);
        }

        private async Task<(bool Success, string OutputFilePath)> CompileContainingProject(string outputDirectory)
        {
            var project = OriginalClass.Project;

            var compilation = (await project.GetCompilationAsync().ConfigureAwait(false))
                .RemoveSyntaxTrees(await OriginalClass.GetSyntaxTreeAsync().ConfigureAwait(false))
                .AddSyntaxTrees(MutatedSyntaxRoot.SyntaxTree);

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