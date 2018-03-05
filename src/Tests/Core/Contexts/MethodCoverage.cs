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

        private readonly Mock<ITestFinder> mockTestFinder = new Mock<ITestFinder>();
        private readonly Mock<ITestRunner> mockTestRunner = new Mock<ITestRunner>();
        private Config config;

        protected IMethodCoverage MethodCoverage { get; private set; }

        protected void Given_an_app_with_tests()
        {
            config = new Config
            {
                SolutionFilePath = Path.Combine(baseExampleDir, "HasSurvivingMutants.sln"),
                SourceFileFilters = new string[0],
                ProjectFilters = new []{ "HasSurvivingMutants.Implementation" },
                TestAssemblyFilePaths = new[]
                {
                    Path.Combine(baseExampleDir, "Tests", "bin", BuildConfig.AsString, "HasSurvivingMutants.Tests.dll")
                }
            };

            mockTestFinder
                .Setup(x => x.FindTests(It.IsAny<IEnumerable<string>>()))
                .Returns(new[] { "HasSurvivingMutants.Tests.PartiallyTestedNumberComparison.IsPositive" });

            mockTestRunner
                .Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new TestRunResult
                {
                    Status = TestRunStatus.AllTestsPassed                    
                });
        }

        protected void When_analysing_method_coverage()
        {
            MethodCoverage = new Fettle.Core.Internal.MethodCoverage(/*mockTestFinder.Object*/new NUnitTestEngine(), /*mockTestRunner.Object*/new NUnitTestEngine());

            MethodCoverage.Initialise(config).Wait();
        }
    }
}
