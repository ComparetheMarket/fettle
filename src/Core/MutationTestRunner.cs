using System.IO;
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
            var baseTempDirectory = TempDirectory.Create();
            try
            {
                CreateTempDirectories(baseTempDirectory, config);
            
                var mutationJobs = await MutationJobList.Create(config, coverageAnalysisResult);
                var survivingMutants = await mutationJobs.RunAll(testRunner, baseTempDirectory, eventListener);
                
                return new MutationTestResult().WithSurvivingMutants(survivingMutants);
            }
            finally
            {
                Directory.Delete(baseTempDirectory, recursive: true);
            }
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