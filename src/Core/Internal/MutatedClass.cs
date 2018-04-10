using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{    
    internal class MutatedClass
    {
        public SyntaxNode MutatedClassRoot { get; }
        public SyntaxNode OriginalNode { get; }
        public Document OriginalClass { get;  }
        
        public MutatedClass(
            SyntaxNode mutatedClassRoot,
            SyntaxNode originalNode,
            Document originalClass)
        {
            MutatedClassRoot = mutatedClassRoot;
            OriginalNode = originalNode;
            OriginalClass = originalClass;
        }

        public async Task<SurvivingMutant> Test(
            MutatedClass mutatedClass,
            Config config,
            ITestRunner testRunner,
            string[] testsToRun,
            string tempDirectory)
        {
            var compilationResult = await mutatedClass.CompileContainingProject(tempDirectory);
            if (!compilationResult.Success)
            {
                // Not all mutations are valid in all circumstances, and therefore may not compile.
                // E.g. "a + b" => "a - b" works when a and b are integers but not when they're strings.
                return null;
            }

            CopyMutatedAssemblyIntoTempTestAssemblyDirectories(compilationResult.OutputFilePath, tempDirectory, config);

            var tempTestAssemblyFilePaths = TempTestAssemblyFilePaths(config, tempDirectory);

            var result = testRunner.RunTests(tempTestAssemblyFilePaths, testsToRun);

            return result.Status == TestRunStatus.AllTestsPassed ?
                await SurvivingMutant.CreateFrom(mutatedClass) :
                null;
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