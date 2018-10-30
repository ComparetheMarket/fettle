using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fettle.Core;
using Fettle.Core.Internal;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Core.Contexts
{
    class Default
    {
        private readonly string baseExampleDir = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "Examples", "HasSurvivingMutants");

        protected Mock<ITestRunner> MockTestRunner { get; }
        private Mock<ICoverageAnalysisResult> mockCoverageAnalysisResult = new Mock<ICoverageAnalysisResult>();

        protected SpyEventListener SpyEventListener { get; } = new SpyEventListener();
        protected MutationTestResult MutationTestResult { get; private set; }
        protected Config Config { get; private set; }
        protected Exception Exception { get; private set; }

        protected string[] TempDirectories => Directory.GetDirectories(Path.GetTempPath(), "*fettle*");

        public Default()
        {        
            Config = new Config
            {
                SolutionFilePath = Path.Combine(baseExampleDir, "HasSurvivingMutants.sln"),
                SourceFileFilters = new []{ @"Implementation\**\*" },
                ProjectFilters = new []{ "HasSurvivingMutants.Implementation" },
                TestProjectFilters = new[]{ "HasSurvivingMutants.Tests" }
            };

            MockTestRunner = new Mock<ITestRunner>();

            mockCoverageAnalysisResult
                .Setup(x => x.TestsThatCoverMember(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new[]{"example.test.one"});

            mockCoverageAnalysisResult.Setup(x => x.IsMemberCovered(It.IsAny<string>())).Returns(true);
        }
        
        protected void Given_a_partially_tested_app_in_which_a_mutant_will_survive()
        {
            var wasCalled = false;

            Func<TestRunResult> returnValueFunction = () =>
            {
                if (!wasCalled)
                {
                    wasCalled = true;
                    return new TestRunResult { Status = TestRunStatus.AllTestsPassed };
                }

                return new TestRunResult { Status = TestRunStatus.SomeTestsFailed };
            };

            MockTestRunner.Setup(x => x.RunAllTests(It.IsAny<IEnumerable<string>>()))
                          .Returns(returnValueFunction);

            MockTestRunner.Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                          .Returns(returnValueFunction);
        }
        
        protected void Given_a_partially_tested_app_in_which_multiple_mutants_survive_for_a_syntax_node()
        {
            MockTestRunner.Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                          .Returns(new TestRunResult { Status = TestRunStatus.AllTestsPassed });
        }

        protected void Given_a_fully_tested_app_in_which_no_mutants_will_survive()
        {
            MockTestRunner.Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                          .Returns(new TestRunResult { Status = TestRunStatus.SomeTestsFailed });
        }
        
        protected void Given_an_app_to_be_mutation_tested()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();
        }
        
        protected void Given_project_filters(params string[] filters)
        {
            Config.ProjectFilters = filters;
        }

        protected void Given_source_file_filters(params string[] filters)
        {
            Config.SourceFileFilters = filters;
        }
        
        protected void Given_there_are_no_pre_existing_temporary_files()
        {
            TempDirectories.ToList().ForEach(d => Directory.Delete(d, recursive: true));
        }

        protected void Given_coverage_analysis_has_not_been_performed()
        {
            mockCoverageAnalysisResult = null;
        }
        
        protected void When_mutation_testing_the_app(bool captureException = false)
        {
            try
            {
                MutationTestResult = new MutationTestRunner(
                            MockTestRunner.Object, 
                            mockCoverageAnalysisResult != null ? mockCoverageAnalysisResult.Object : null, 
                            SpyEventListener)
                        .Run(Config).Result;
            }
            catch (Exception e)
            {
                Exception = e;

                if (!captureException)
                    throw;
            }
        }
    }
}