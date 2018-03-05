using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Missing_command_line_arguments : Contexts.Default
    {
        public Missing_command_line_arguments()
        {
            Given_no_command_line_arguments_specified();

            When_running_the_fettle_console_app();
        }
        
        [Test]
        public void Then_an_error_message_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenFailureLines, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Then_the_exit_code_indicates_that_the_app_failed()
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