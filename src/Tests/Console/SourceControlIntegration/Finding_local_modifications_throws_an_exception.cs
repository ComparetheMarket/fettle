using System;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Console.SourceControlIntegration
{
    class Finding_local_modifications_throws_an_exception : Contexts.SourceControlIntegration
    {
        public Finding_local_modifications_throws_an_exception()
        {
            Given_a_valid_config_file();
            Given_additional_command_line_arguments("--modificationsonly");
            Given_locally_modified_files_will_throw_an_exception(new InvalidOperationException("some source control error"));

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_an_error_message_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenFailureLines, Has.Count.GreaterThan(0));
            Assert.That(SpyOutputWriter.WrittenFailureLines.SingleOrDefault(m => 
                    m.Contains("some source control error")),
                Is.Not.Null);
        }

        [Test]
        public void Then_the_exit_code_indicates_that_the_app_failed()
        {
            Assert.That(ExitCode, Is.EqualTo(3));
        }
    }
}