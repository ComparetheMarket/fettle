using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Unrecognised_command_line_arguments : Contexts.Default
    {
        public Unrecognised_command_line_arguments()
        {
            Given_a_valid_config_file();
            Given_additional_command_line_arguments("--flange", "yarp");

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_an_error_message_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenFailureLines, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Then_the_exit_code_indicates_that_the_arguments_were_invalid()
        {
            Assert.That(ExitCode, Is.EqualTo(2));
        }

        [Test]
        public void Then_mutation_testing_is_not_attempted()
        {
            MockMutationTestRunner.Verify(
                mtr => mtr.Run(It.IsAny<Config>()),
                Times.Never);
        }
    }
}