﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using Microsoft.CodeAnalysis;

namespace Fettle.Core
{
    public class CoverageAnalyser : ICoverageAnalyser
    {
        private readonly IEventListener eventListener;
        private readonly ITestFinder testFinder;
        private readonly ITestRunner testRunner;

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
                var modifiedSyntaxTree = await Instrumentation.InstrumentDocument(
                    originalSyntaxTree, 
                    originalClass,
                    methodIdsToNames.Add);

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
