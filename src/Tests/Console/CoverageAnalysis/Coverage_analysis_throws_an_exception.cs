using System;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Console.CoverageAnalysis
{
    class Coverage_analysis_throws_an_exception : Contexts.Default
    {
        public Coverage_analysis_throws_an_exception()
        {
            Given_a_valid_config_file();

            Given_coverage_analysis_will_throw_an_exception(
                new InvalidOperationException("something went wrong during coverage analysis"));

            When_running_the_fettle_console_app();
        }
        
        [Test]
        public void Then_an_error_message_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenFailureLines, Has.Count.GreaterThan(0));
            Assert.That(SpyOutputWriter.WrittenFailureLines.SingleOrDefault(m => 
                    m.Contains("An error ocurred that Fettle didn't expect")),
                Is.Not.Null);
            Assert.That(SpyOutputWriter.WrittenFailureLines.SingleOrDefault(m => 
                    m.Contains("something went wrong during coverage analysis")),
                Is.Not.Null);
        }

        [Test]
        public void Then_the_exit_code_indicates_that_the_app_failed()
        {
            Assert.That(ExitCode, Is.EqualTo(3));
        }
    }
}