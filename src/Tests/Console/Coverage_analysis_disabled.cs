using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Coverage_analysis_disabled : Contexts.Default
    {
        public Coverage_analysis_disabled()
        {
            Given_a_valid_config_file();
            Given_coverage_analysis_is_disabled_via_command_line_argument();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_the_argument_to_disable_coverage_analysis_is_a_valid()
        {
            Assert.That(ExitCode, Is.Not.EqualTo(2));
        }

        [Test]
        public void Then_coverage_analysis_is_not_performed()
        {
            MockCoverageAnalyser.Verify(
                analyser => analyser.AnalyseCoverage(It.IsAny<Config>()),
                Times.Never);
        }

        [Test]
        public void Then_mutation_testing_is_still_performed()
        {
            MockMutationTestRunner.Verify(
                mtr => mtr.Run(It.IsAny<Config>()),
                Times.Once);
        }
    }
}