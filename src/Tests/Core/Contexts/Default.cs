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
        private Mock<ITestRunner> mockTestRunner;

        private readonly string baseExampleDir = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "Examples", "HasSurvivingMutants");

        protected SpyEventListener SpyEventListener { get; } = new SpyEventListener();
        protected Result Result { get; private set; }
        protected Config Config { get; }
        protected Exception Exception { get; private set; }
                
        protected string[] TempDirectories => Directory.GetDirectories(Path.GetTempPath(), "*fettle*");

        public Default()
        {        
            Config = new Config
            {
                SolutionFilePath = Path.Combine(baseExampleDir, "HasSurvivingMutants.sln"),
                SourceFileFilters = new []{ @"Implementation\**\*" },
                ProjectFilters = new []{ "HasSurvivingMutants.Implementation" },
                TestAssemblyFilePaths = new[]
                {
                    Path.Combine(baseExampleDir, "Tests", "bin", BuildConfig.AsString, "HasSurvivingMutants.Tests.dll")
                }
            };
        }
        
        protected void Given_a_partially_tested_app_in_which_a_mutant_will_survive()
        {
            mockTestRunner = new Mock<ITestRunner>();
            var wasCalled = false;

            mockTestRunner.Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>()))
                          .Returns(() =>
                          {
                              if (!wasCalled)
                              {
                                  wasCalled = true;
                                  return TestRunnerResult.AllTestsPassed;
                              }

                              return TestRunnerResult.SomeTestsFailed;
                          });
        }
        
        protected void Given_a_partially_tested_app_in_which_multiple_mutants_survive_for_a_syntax_node()
        {
            mockTestRunner = new Mock<ITestRunner>();
            mockTestRunner.Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>()))
                          .Returns(TestRunnerResult.AllTestsPassed);
        }

        protected void Given_a_fully_tested_app_in_which_no_mutants_will_survive()
        {
            mockTestRunner = new Mock<ITestRunner>();
            mockTestRunner.Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>()))
                          .Returns(TestRunnerResult.SomeTestsFailed);
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

        protected void Given_a_coverage_report(string coveragereportFilename)
        {
            if (coveragereportFilename != null)
            {
                Config.CoverageReportFilePath =
                    Path.Combine(
                        TestContext.CurrentContext.TestDirectory,
                        "Core", "CoverageReport", "TestData",
                        coveragereportFilename);
            }
        }

        protected void Given_there_are_no_pre_existing_temporary_files()
        {
            TempDirectories.ToList().ForEach(d => Directory.Delete(d, recursive: true));
        }

        protected void When_mutation_testing_the_app(bool captureException = false)
        {
            try
            {
                Result = new MutationTestRunner(mockTestRunner.Object, SpyEventListener)
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