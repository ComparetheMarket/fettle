using Fettle.Core;
using Moq;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Custom_test_runner_specified_and_coverage_analysis_skipped : Contexts.Default
    {
        public Custom_test_runner_specified_and_coverage_analysis_skipped()
        {
            Given_a_config_file_with_custom_test_runner_specified();
            Given_coverage_analysis_is_disabled_via_command_line_argument();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_user_is_not_warned_that_coverage_analysis_cannot_be_performed_as_they_have_asked_to_skip_it_anyway()
        {
            Assert.That(SpyOutputWriter.WrittenWarningLines, Has.Count.Zero);
        }

        [Test]
        public void Then_coverage_analysis_is_not_attempted()
        {
            MockCoverageAnalyser.Verify(x => x.AnalyseCoverage(It.IsAny<Config>()), Times.Never);
        }
    }

    class Custom_test_runner_command_specified_but_coverage_analysis_not_skipped : Contexts.Default
    {
        public Custom_test_runner_command_specified_but_coverage_analysis_not_skipped()
        {
            Given_a_config_file_with_custom_test_runner_specified();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_user_is_warned_that_coverage_analysis_cannot_be_performed()
        {
            Assert.That(SpyOutputWriter.WrittenWarningLines.SingleOrDefault()?.ToLowerInvariant(), Does.Contain("coverage analysis"));
        }

        [Test]
        public void Then_coverage_analysis_is_not_attempted()
        {
            MockCoverageAnalyser.Verify(x => x.AnalyseCoverage(It.IsAny<Config>()), Times.Never);
        }
    }
}
