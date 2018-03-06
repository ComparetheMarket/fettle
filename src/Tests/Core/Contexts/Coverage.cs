using System.Collections.Generic;
using System.IO;
using Fettle.Core;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Core.Contexts
{
    class Coverage
    {
        private readonly string baseExampleDir = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "Examples", "HasSurvivingMutants");

        private ITestRunner testRunner = new NUnitTestEngine();

        private Config config;

        protected CoverageAnalysisResult Result { get; private set;}

        protected void Given_an_app_with_tests()
        {
            config = new Config
            {
                SolutionFilePath = Path.Combine(baseExampleDir, "HasSurvivingMutants.sln"),
                SourceFileFilters = new string[0],
                TestAssemblyFilePaths = new[]
                {
                    Path.Combine(baseExampleDir, "Tests", "bin", BuildConfig.AsString, "HasSurvivingMutants.Tests.dll")
                }
            };
        }

        protected void Given_some_failing_tests()
        {
            var mockTestRunner = new Mock<ITestRunner>();
            mockTestRunner
                .Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new TestRunResult{ Status = TestRunStatus.SomeTestsFailed });

            testRunner = mockTestRunner.Object;
        }

        protected void Given_project_filters(params string[] projectFilters)
        {
            config.ProjectFilters = projectFilters;
        }

        protected void When_analysing_method_coverage()
        {
            var methodCoverage = new Fettle.Core.MethodCoverage(
                testFinder: new NUnitTestEngine(), 
                testRunner: testRunner);

            Result = methodCoverage.AnalyseMethodCoverage(config).Result;
        }
    }
}
