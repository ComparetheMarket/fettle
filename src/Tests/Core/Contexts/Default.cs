using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private readonly Dictionary<string, ImmutableHashSet<string>> methodsAndTheirCoveringTests;

        private readonly string baseExampleDir = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "Examples", "HasSurvivingMutants");

        protected Mock<ITestRunner> MockTestRunner { get; }

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
                TestAssemblyFilePaths = new[]
                {
                    Path.Combine(baseExampleDir, "Tests", "bin", BuildConfig.AsString, "HasSurvivingMutants.Tests.dll")
                }
            };

            MockTestRunner = new Mock<ITestRunner>();

            var methodNames = new []
            {
                "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)",
                "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsNegative(System.Int32)",
                "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsZero(System.Int32)",
                "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::AreBothZero(System.Int32,System.Int32)",
                "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Sum(System.Int32,System.Int32)",
                "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Preincrement(System.Int32)",
                "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Postincrement(System.Int32)",
                "System.String HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::PositiveOrNegative(System.Int32)",
                "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::AddNumbers_should_be_ignored(System.Int32)"
            };
            methodsAndTheirCoveringTests = new Dictionary<string, ImmutableHashSet<string>>();
            foreach (var methodName in methodNames)
            {
                methodsAndTheirCoveringTests.Add(methodName, ImmutableHashSet<string>.Empty.Add("example.test.one"));
            }
        }
        
        protected void Given_a_partially_tested_app_in_which_a_mutant_will_survive()
        {
            var wasCalled = false;

            MockTestRunner.Setup(x => x.RunTests(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                          .Returns(() =>
                          {
                              if (!wasCalled)
                              {
                                  wasCalled = true;
                                  return new TestRunResult { Status = TestRunStatus.AllTestsPassed };
                              }

                              return new TestRunResult { Status = TestRunStatus.SomeTestsFailed };
                          });
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

        protected void Given_config_is_invalid(Func<Config, Config> configModifier)
        {
            Config = configModifier(Config);
        }
        
        protected void When_mutation_testing_the_app(bool captureException = false)
        {
            try
            {
                MutationTestResult = new MutationTestRunner(
                            MockTestRunner.Object, 
                            methodsAndTheirCoveringTests, 
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