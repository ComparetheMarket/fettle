﻿using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Verbose_mode_enabled : Contexts.Default
    {
        public Verbose_mode_enabled()
        {
            Given_a_valid_config_file();

            Given_coverage_analysis_runs_successfully();
            Given_some_mutants_will_survive();

            Given_additional_command_line_arguments("--verbose");

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Verbose_mode_is_a_valid_command_line_argument()
        {
            Assert.That(ExitCode, Is.Not.EqualTo(2));
        }

        [Test]
        public void Then_detailed_coverage_analysis_progress_is_output()
        {
            Assert.That(SpyOutputWriter.AllOutput, Does.Contain("is covered").IgnoreCase);
        }

        [Test]
        public void Then_detailed_mutation_testing_progress_is_output()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SpyOutputWriter.AllOutput, Does.Contain("a > 0").And.Contains("a >= 0"));
                Assert.That(SpyOutputWriter.AllOutput, Does.Contain("killed").IgnoreCase);
                Assert.That(SpyOutputWriter.AllOutput, Does.Contain("skipped").And.Contains("skip reason").IgnoreCase);
            });
        }
    }
    
    class Verbose_mode_disabled : Contexts.Default
    {
        public Verbose_mode_disabled()
        {
            Given_a_valid_config_file();

            Given_coverage_analysis_runs_successfully();
            Given_some_mutants_will_survive();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Verbose_mode_is_not_a_required_command_line_argument()
        {
            Assert.That(ExitCode, Is.Not.EqualTo(2));
        }

        [Test]
        public void Then_detailed_coverage_analysis_progress_is_not_output()
        {
            Assert.That(SpyOutputWriter.AllOutput, Does.Not.Contain("is covered").IgnoreCase);
        }

        [Test]
        public void Then_detailed_mutation_testing_progress_is_not_output()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SpyOutputWriter.AllOutput, Does.Not.Contain("a > 0").Or.Contains("a >= 0"));
                Assert.That(SpyOutputWriter.AllOutput, Does.Not.Contain("killed").IgnoreCase);
                Assert.That(SpyOutputWriter.AllOutput, Does.Not.Contain("skipped").IgnoreCase);
            });
        }
    }
}
