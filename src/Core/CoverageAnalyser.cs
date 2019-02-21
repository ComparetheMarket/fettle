using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.Instrumentation;
using Fettle.Core.Internal.NUnit;
using Microsoft.CodeAnalysis;

namespace Fettle.Core
{
    public class CoverageAnalyser : ICoverageAnalyser
    {
        private readonly IEventListener eventListener;
        private readonly ITestFinder testFinder;
        private readonly ICoverageTestRunner testRunner;

        public CoverageAnalyser(IEventListener eventListener) : 
            this(eventListener, new NUnitTestFinder(), new NUnitCoverageTestRunner())
        {
        }

        internal CoverageAnalyser(IEventListener eventListener, ITestFinder testFinder, ICoverageTestRunner testRunner)
        {
            this.eventListener = eventListener;
            this.testFinder = testFinder;
            this.testRunner = testRunner;
        }

        public async Task<ICoverageAnalysisResult> AnalyseCoverage(Config config)
        {
            var memberIdsToNames = new Dictionary<string, string>();
            
            var baseTempDirectory = TempDirectory.Create();

            long memberId = 0;
            long GenerateMemberId() => ++memberId;

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
                        GenerateMemberId,
                        memberIdsToNames);

                    var result = new CoverageAnalysisResult();

                    for (int testAssemblyIndex = 0; testAssemblyIndex < config.TestAssemblyFilePaths.Length; ++testAssemblyIndex)
                    {
                        var copiedTestAssemblyFilePath = copiedTestAssemblyFilePaths[testAssemblyIndex];

                        var numTests = testFinder.FindTests(new[]{ copiedTestAssemblyFilePath }).Length;

                        var runResult = testRunner.RunAllTestsAndAnalyseCoverage(
                            testAssemblyFilePaths: new [] { copiedTestAssemblyFilePath },
                            memberIdsToNames: memberIdsToNames, 
                            onAnalysingTestCase: (test, index) => eventListener.BeginCoverageAnalysisOfTestCase(test, index, numTests),
                            onMemberExecuted: memberName => eventListener.MemberCoveredByTests(memberName));

                        if (runResult.Status != TestRunStatus.AllTestsPassed)
                        {
                            return CoverageAnalysisResult.Error(runResult.Error);
                        }

                        var originalTestAssemblyFilePath = config.TestAssemblyFilePaths[testAssemblyIndex];
                        result = result.WithCoveredMembers(runResult.MembersAndCoveringTests, originalTestAssemblyFilePath);
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
            Func<long> memberIdGenerator,
            IDictionary<string, string> memberIdsToNames)
        {
            foreach (var project in projects)
            {
                var outputFilePath = Path.Combine(baseTempDirectory, $@"{project.AssemblyName}.dll");

                await InstrumentThenCompileProject(project, config, outputFilePath, memberIdGenerator, memberIdsToNames);

                CopyInstrumentedAssemblyIntoTempTestAssemblyDirectories(
                    outputFilePath, 
                    copiedTestAssemblyFilePaths.Select(Path.GetDirectoryName));
            }
        }

        private static async Task InstrumentThenCompileProject(
            Project project,
            Config config,
            string outputFilePath,
            Func<long> memberIdGenerator,
            IDictionary<string, string> memberIdsToNames)
        {
            var originalSyntaxTrees = new List<SyntaxTree>();
            var modifiedSyntaxTrees = new List<SyntaxTree>();

            var classesToInstrument = project.Documents
                .Where(d => Filtering.ShouldMutateDocument(d, config))
                .Where(d => !d.IsAutomaticallyGenerated());

            foreach (var originalClass in classesToInstrument)
            {
                var originalSyntaxTree = await originalClass.GetSyntaxTreeAsync().ConfigureAwait(false);
                var modifiedSyntaxTree = await Instrumentation.InstrumentDocument(
                    originalSyntaxTree, 
                    originalClass,
                    memberIdsToNames.Add,
                    memberIdGenerator);

                originalSyntaxTrees.Add(originalSyntaxTree);
                modifiedSyntaxTrees.Add(modifiedSyntaxTree);
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
