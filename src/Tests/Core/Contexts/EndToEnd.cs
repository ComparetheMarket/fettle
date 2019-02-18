using System.IO;
using Fettle.Console;
using Fettle.Core;
using Fettle.Tests.Console.ImplementationDetails;
using NUnit.Framework;

namespace Fettle.Tests.Core.Contexts
{
    class EndToEnd
    {
        protected MutationTestResult MutationTestResult { get; private set; }
        private ITestRunner testRunner;
        private ICoverageAnalysisResult coverageAnalysisResult;
        private readonly Config config = new Config();

        private static string BaseDir => Path.Combine(TestContext.CurrentContext.TestDirectory,
                "..", "..", "..", "Examples", "HasSurvivingMutants");

        protected void Given_an_app_which_has_nunit_tests()
        {
            Given_an_app();

            config.ProjectFilters = new[] {"HasSurvivingMutants.Implementation"};
            config.SourceFileFilters = new[] {@"Implementation\*"};

            var binDir = Path.Combine(BaseDir, "Tests", "bin", BuildConfig.AsString);
            config.TestAssemblyFilePaths = new[]
            {
                Path.Combine(binDir, "HasSurvivingMutants.Tests.dll")
            };
            config.CustomTestRunnerCommand = null;

            testRunner = new TestRunnerFactory().CreateNUnitTestRunner();
            coverageAnalysisResult = new CoverageAnalyser(new SpyEventListener()).AnalyseCoverage(config).Result;
        }

        protected void Given_an_app_which_has_a_custom_test_runner()
        {
            Given_an_app();
 
            config.ProjectFilters = new[] {"HasSurvivingMutants.MoreImplementation"};
            config.SourceFileFilters = new[] {@"MoreImplementation\MorePartiallyTestedNumberComparison.cs"};

            var binDir = Path.Combine(BaseDir, "XUnitTests", "bin", BuildConfig.AsString);
            config.TestAssemblyFilePaths = new[]
            {
                Path.Combine(binDir, "HasSurvivingMutants.XUnitTests.dll")
            };
            config.CustomTestRunnerCommand = @"XUnitTests\run-example-xunit-tests.bat";

            testRunner = new TestRunnerFactory().CreateCustomTestRunner(config.CustomTestRunnerCommand, BaseDir);
            coverageAnalysisResult = null;
        }

        protected void When_mutation_testing_the_app()
        {
            MutationTestResult = new MutationTestRunner(testRunner, coverageAnalysisResult)
                .Run(config)
                .Result;
        }

        private void Given_an_app()
        {
            config.SolutionFilePath = Path.Combine(BaseDir, "HasSurvivingMutants.sln");
        }
    }
}