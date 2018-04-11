using System;
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
            "..", "..", "..", "Examples");

        private ITestRunner testRunner = new NUnitTestEngine();

        protected Config Config { get; private set; }

        protected CoverageAnalysisResult Result { get; private set;}
        protected Exception ThrownException { get; private set; }
        protected Mock<IEventListener> MockEventListener { get; } = new Mock<IEventListener>();

        protected void Given_an_app_with_tests()
        {
            Config = new Config
            {
                SolutionFilePath = Path.Combine(baseExampleDir, "HasSurvivingMutants", "HasSurvivingMutants.sln"),
                SourceFileFilters = new string[0],
                TestAssemblyFilePaths = new[]
                {
                    Path.Combine(baseExampleDir, "HasSurvivingMutants", "Tests", "bin", BuildConfig.AsString, "HasSurvivingMutants.Tests.dll"),
                    Path.Combine(baseExampleDir, "HasSurvivingMutants", "MoreTests", "bin", BuildConfig.AsString, "HasSurvivingMutants.MoreTests.dll")
                }
            };
        }

        protected void Given_an_app_that_does_not_compile()
        {
            Config = new Config
            {
                SolutionFilePath = Path.Combine(baseExampleDir, "DoesNotCompile", "DoesNotCompile.sln"),
                SourceFileFilters = new string[0],
                TestAssemblyFilePaths = new[]
                {
                    Path.Combine(baseExampleDir, "DoesNotCompile", "Tests", "bin", BuildConfig.AsString, "Tests.dll")
                }
            };
        }

        protected void Given_some_failing_tests()
        {
            var mockTestRunner = new Mock<ITestRunner>();
            mockTestRunner
                .Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new TestRunResult
                {
                    Status = TestRunStatus.SomeTestsFailed,
                    Error = "HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred failed"
                });

            mockTestRunner
                .Setup(x => x.RunTestsAndAnalyseCoverage(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IDictionary<string,string>>(),
                    It.IsAny<Action<string,int>>()))
                .Returns(new CoverageTestRunResult
                {
                    Status = TestRunStatus.SomeTestsFailed,
                    Error = "HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred failed"
                });

            testRunner = mockTestRunner.Object;
        }

        protected void Given_project_filters(params string[] projectFilters)
        {
            Config.ProjectFilters = projectFilters;
        }

        protected void Given_source_file_filters(params string[] sourceFileFilters)
        {
            Config.SourceFileFilters = sourceFileFilters;
        }

        protected void When_analysing_method_coverage(bool catchExceptions = false)
        {
            try
            {
                var methodCoverage = new Fettle.Core.CoverageAnalyser(
                    eventListener: MockEventListener.Object,
                    testFinder: new NUnitTestEngine(), 
                    testRunner: testRunner);

                Result = methodCoverage.AnalyseMethodCoverage(Config).Result;
            }
            catch (Exception e)
            {
                if (catchExceptions)
                    ThrownException = e;
                else
                    throw;
            }
        }
    }
}
