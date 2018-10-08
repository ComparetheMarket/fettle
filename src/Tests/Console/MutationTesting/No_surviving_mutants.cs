using NUnit.Framework;

namespace Fettle.Tests.Console.MutationTesting
{
    class No_surviving_mutants : Contexts.Default
    {
        public No_surviving_mutants()
        {
            Given_a_valid_config_file();
            Given_no_mutants_will_survive();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_the_exit_code_indicates_that_no_mutants_survived()
        {
            Assert.That(ExitCode, Is.EqualTo(0));
        }

        [Test]
        public void Then_a_successful_message_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenSuccessLines, Has.Count.GreaterThan(0));
            Assert.That(SpyOutputWriter.WrittenFailureLines, Has.Count.Zero);
        }
    }
}