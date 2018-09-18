using System;
using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    [TestFixtureSource(nameof(TestCases))]
    class Invalid_config : Contexts.InvalidConfig
    {
        private static readonly object[] TestCases =
        {
            new object[] { new Func<Config,Config>(WithNonExistentSolutionFile) },
            new object[] { new Func<Config,Config>(WithNonExistentTestAssembly) },
            
            new object[] { new Func<Config,Config>(WithNullTestAssembly) },
            new object[] { new Func<Config,Config>(WithNullProjectFilter) },
            new object[] { new Func<Config,Config>(WithNullSourceFileFilter) },

            new object[] { new Func<Config,Config>(WithNoSolutionFile) },
            new object[] { new Func<Config,Config>(WithNoTestAssemblies) }
        };

        public Invalid_config(Func<Config, Config> modifier)
        {
            Given_a_config_file_with_invalid_contents(modifier);

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_the_exit_code_indicates_that_the_configuration_is_invalid()
        {
             Assert.That(ExitCode, Is.EqualTo(2));
        }

        [Test]
        public void Then_error_details_are_output()
        {
            Assert.That(SpyOutputWriter.WrittenFailureLines, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Then_coverage_analysis_is_not_performed()
        {
            MockCoverageAnalyser.Verify(x => x.AnalyseCoverage(It.IsAny<Config>()), Times.Never);
        }

        [Test]
        public void Then_mutation_testing_is_not_performed()
        {
            MockMutationTestRunner.Verify(x => x.Run(It.IsAny<Config>()), Times.Never);
        }
    }
}
