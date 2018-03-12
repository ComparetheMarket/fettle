using System.IO;
using Fettle.Core;
using NUnit.Framework;

namespace Fettle.Tests.Core.Contexts
{
    class EndToEnd
    {
        private readonly Config config = new Config
        {
            ProjectFilters = new []{ "HasSurvivingMutants.Implementation" },
            SourceFileFilters = new [] { @"Implementation\*" }
        };

        protected MutationTestResult MutationTestResult { get; private set; }
        
        protected void Given_an_app_which_has_gaps_in_its_tests()
        {
            var baseDir = Path.Combine(TestContext.CurrentContext.TestDirectory,
                "..", "..", "..", "Examples", "HasSurvivingMutants");

            config.SolutionFilePath = Path.Combine(baseDir, "HasSurvivingMutants.sln");

            var binDir = Path.Combine(baseDir, "Tests", "bin", BuildConfig.AsString);

            config.TestAssemblyFilePaths = new[]
            {
                Path.Combine(binDir, "HasSurvivingMutants.Tests.dll")
            };
        }
        
        protected void When_mutation_testing_the_app()
        {
            var coverageAnalysisResult = new Fettle.Core.MethodCoverage(new SpyEventListener())
                .AnalyseMethodCoverage(config).Result;

            MutationTestResult = new MutationTestRunner(coverageAnalysisResult.MethodsAndTheirCoveringTests)
                .Run(config)
                .Result;
        }
    }
}