using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;

namespace Fettle.Core
{
    public class MutationTestRunner : IMutationTestRunner
    {
        private readonly ITestRunner testRunner;
        private readonly ICoverageAnalysisResult coverageAnalysisResult;
        private readonly IEventListener eventListener;

        public MutationTestRunner(
            ICoverageAnalysisResult coverageAnalysisResult,
            IEventListener eventListener = null) :
            this(new NUnitTestEngine(), coverageAnalysisResult, eventListener)
        {
        }

        internal MutationTestRunner(
            ITestRunner testRunner,
            ICoverageAnalysisResult coverageAnalysisResult,
            IEventListener eventListener = null)
        {
            this.testRunner = testRunner;
            this.coverageAnalysisResult = coverageAnalysisResult;
            this.eventListener = eventListener ?? new NullEventListener();
        }

        public async Task<MutationTestResult> Run(Config config)
        {
            var validationErrors = config.Validate().ToList();
            if (validationErrors.Any())
            {
                return new MutationTestResult().WithErrors(validationErrors);
            }

            var baseTempDirectory = TempDirectory.Create();
            try
            {
                CreateTempDirectories(baseTempDirectory, config);
            
                var mutationJobs = await CreateMutationJobs(config);
                var survivingMutants = await mutationJobs.RunAll(config, testRunner, baseTempDirectory, eventListener);
                
                return new MutationTestResult().WithSurvivingMutants(survivingMutants);
            }
            finally
            {
                Directory.Delete(baseTempDirectory, recursive: true);
            }
        }

        private async Task<MutationJobList> CreateMutationJobs(Config config)
        {
            var jobs = new MutationJobList();

            using (var workspace = MSBuildWorkspaceFactory.Create())
            {
                var solution = await workspace.OpenSolutionAsync(config.SolutionFilePath);

                var classesToMutate = solution.MutatableClasses(config);
                for (var classIndex = 0; classIndex < classesToMutate.Length; classIndex++)
                {
                    var classToMutate = classesToMutate[classIndex];
                    var classRoot = await classToMutate.GetSyntaxRootAsync();
                    var documentSemanticModel = await classToMutate.GetSemanticModelAsync();

                    var nodesToMutate = classRoot.DescendantNodes().ToArray();
                    var isIgnoring = false;

                    for (var nodeIndex = 0; nodeIndex < nodesToMutate.Length; nodeIndex++)
                    {
                        var nodeToMutate = nodesToMutate[nodeIndex];

                        var methodName = nodeToMutate.NameOfContainingMethod(documentSemanticModel);
                        if (methodName == null)
                        {
                            // The node is not within a method, e.g. it's a class or namespace declaration.
                            // Therefore there is no code to mutate.
                            continue;
                        }

                        if (!coverageAnalysisResult.IsMethodCovered(methodName))
                        {
                            continue;
                        }

                        if (Ignoring.NodeHasBeginIgnoreComment(nodeToMutate)) isIgnoring = true;
                        else if (Ignoring.NodeHasEndIgnoreComment(nodeToMutate)) isIgnoring = false;

                        if (isIgnoring)
                        {
                            continue;
                        }

                        foreach (var mutator in nodeToMutate.SupportedMutators())
                        {
                            var job = new MutationJob(
                                classRoot,
                                nodeToMutate,
                                classToMutate,
                                methodName,
                                config,
                                mutator,
                                coverageAnalysisResult);

                            var jobMetadata = new MutationJobMetadata
                            {
                                SourceFilePath = classToMutate.FilePath,
                                SourceFileIndex = classIndex,
                                SourceFilesTotal = classesToMutate.Length,

                                MethodName = methodName,
                                
                                SyntaxNodeIndex = nodeIndex,
                                SyntaxNodesTotal = nodesToMutate.Length
                            };

                            jobs.AddJob(job, jobMetadata);
                        }
                    }
                }
            }

            return jobs;
        }

        private static void CreateTempDirectories(string baseTempDirectory, Config config)
        {
            foreach (var testAssemblyFilePath in config.TestAssemblyFilePaths)
            {
                var from = Path.GetDirectoryName(testAssemblyFilePath);
                var to = Path.Combine(baseTempDirectory, Path.GetFileNameWithoutExtension(testAssemblyFilePath));
                Directory.CreateDirectory(to);
                DirectoryUtils.CopyDirectoryContents(from, to);
            }
        }
    }
}