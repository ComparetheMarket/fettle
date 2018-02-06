using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Surviving_mutants : Contexts.Default
    {
        public Surviving_mutants()
        {
            Given_a_valid_config_file();
            Given_some_mutants_will_survive();

            When_running_the_fettle_console_app();
        }
        
        [Test]
        public void Then_an_error_message_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenFailureLines, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Then_the_exit_code_indicates_that_mutants_survived()
        {
            Assert.That(ExitCode, Is.EqualTo(1));
        }
    }
}