using Fettle.Core;
using Moq;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Custom_test_runner_specified : Contexts.Default
    {
        public Custom_test_runner_specified()
        {
            Given_a_config_file_with_custom_test_runner_specified();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_coverage_analysis_is_not_attempted_because_it_is_incompatible()
        {
            MockCoverageAnalyser.Verify(x => x.AnalyseCoverage(It.IsAny<Config>()), Times.Never);
        }

        [Test]
        public void Then_the_custom_test_runner_is_used_instead_of_the_default_nunit_one()
        {
            MockTestRunnerFactory.Verify(x => x.CreateCustomTestRunner(), Times.Once);
            MockTestRunnerFactory.Verify(x => x.CreateNUnitTestRunner(), Times.Never);
        }
    }

    class Custom_test_runner_not_specified : Contexts.Default
    {
        public Custom_test_runner_not_specified()
        {
            Given_a_valid_config_file();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_coverage_analysis_is_attempted_because_it_is_compatible()
        {
            MockCoverageAnalyser.Verify(x => x.AnalyseCoverage(It.IsAny<Config>()), Times.Once);
        }

        [Test]
        public void Then_the_default_nunit_test_runner_is_used_instead_of_the_custom_one()
        {
            MockTestRunnerFactory.Verify(x => x.CreateNUnitTestRunner(), Times.Once);
            MockTestRunnerFactory.Verify(x => x.CreateCustomTestRunner(), Times.Never);
        }
    }

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
    }
}
