using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Console.MutationTesting
{
    class Errors_occur_during_mutation_testing : Contexts.Default
    {
        public Errors_occur_during_mutation_testing()
        {
            Given_mutation_testing_will_return_errors();

            When_running_the_fettle_console_app();
        }
        
        [Test]
        public void Then_an_error_message_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenFailureLines, Has.Count.GreaterThan(0));
            Assert.That(SpyOutputWriter.WrittenFailureLines.Any(l => l.Contains("an example error")), Is.True);
        }

        [Test]
        public void Then_the_exit_code_indicates_that_the_app_failed()
        {
            Assert.That(ExitCode, Is.EqualTo(2));
        }
    }
}