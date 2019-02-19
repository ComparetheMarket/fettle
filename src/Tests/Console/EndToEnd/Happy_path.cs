using NUnit.Framework;

namespace Fettle.Tests.Console.EndToEnd
{
    [TestFixture]
    public class Happy_path : Contexts.EndToEnd
    {
        public Happy_path()
        {
            When_running_real_fettle_console_executable(configFilename: "fettle.config.yml");
        }

        [Test]
        public void Console_executable_runs_and_outputs_info_about_surviving_mutants()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ExitCode, Is.EqualTo(1));
                Assert.That(StandardError, Is.Empty);
                Assert.That(StandardOutput, Does.Contain("mutant(s) survived!"));
                Assert.That(StandardOutput, Does.Contain("PartiallyTestedNumberComparison.cs:7"));    
            });
        }
    }
}
