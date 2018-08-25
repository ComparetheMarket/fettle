using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Quiet_mode_enabled : Contexts.Default
    {
        public Quiet_mode_enabled()
        {
            Given_a_valid_config_file();

            Given_coverage_analysis_runs_successfully();
            Given_some_mutants_will_survive();

            Given_additional_command_line_arguments("--quiet");

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Quiet_mode_is_a_valid_command_line_argument()
        {
            Assert.That(ExitCode, Is.Not.EqualTo(2));
        }

        [Test]
        public void Then_mutation_testing_progress_is_not_output()
        {
            Assert.That(SpyOutputWriter.WrittenNormalLines.Concat(SpyOutputWriter.WrittenLineSegments)
                    .Any(l => l.Contains(".cs")),
                Is.False);
        }

        [Test]
        public void Then_coverage_analysis_progress_is_not_output()
        {
            Assert.That(string.Join("", SpyOutputWriter.WrittenLineSegments), Does.Not.Contain(".."));
        }
    }
    
    class Quiet_mode_disabled : Contexts.Default
    {
        public Quiet_mode_disabled()
        {
            Given_a_valid_config_file();

            Given_coverage_analysis_runs_successfully();
            Given_some_mutants_will_survive();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Quiet_mode_is_not_a_required_command_line_argument()
        {
            Assert.That(ExitCode, Is.Not.EqualTo(2));
        }

        [Test]
        public void Then_mutation_testing_progress_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenNormalLines.Concat(SpyOutputWriter.WrittenLineSegments)
                    .Any(l => l.Contains(".cs")),
                Is.True);
        }

        [Test]
        public void Then_coverage_analysis_progress_is_output()
        {
            Assert.That(string.Join("", SpyOutputWriter.WrittenLineSegments), Does.Contain(".."));
        }
    }
}
