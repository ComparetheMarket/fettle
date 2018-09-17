using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using Fettle.Core.Internal.RoslynExtensions;

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

                var documentsToMutate = solution.MutatableClasses(config);
                for (var classIndex = 0; classIndex < documentsToMutate.Length; classIndex++)
                {
                    var documentToMutate = documentsToMutate[classIndex];
                    var documentSyntaxRoot = await documentToMutate.GetSyntaxRootAsync();
                    var documentSemanticModel = await documentToMutate.GetSemanticModelAsync();

                    var nodesToMutate = documentSyntaxRoot.DescendantNodes().ToArray();
                    var isIgnoring = false;

                    for (var nodeIndex = 0; nodeIndex < nodesToMutate.Length; nodeIndex++)
                    {
                        var nodeToMutate = nodesToMutate[nodeIndex];

                        var memberName = nodeToMutate.NameOfContainingMember(documentSemanticModel);
                        if (memberName == null)
                        {
                            // The node is not within a member, e.g. it's a class or namespace declaration.
                            // Therefore there is no code to mutate.
                            continue;
                        }

                        if (coverageAnalysisResult != null &&
                            !coverageAnalysisResult.IsMemberCovered(memberName))
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
                                documentSyntaxRoot,
                                nodeToMutate,
                                documentToMutate,
                                memberName,
                                config,
                                mutator,
                                coverageAnalysisResult);

                            var jobMetadata = new MutationJobMetadata
                            {
                                SourceFilePath = documentToMutate.FilePath,
                                SourceFileIndex = classIndex,
                                SourceFilesTotal = documentsToMutate.Length,

                                MemberName = memberName,
                                
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