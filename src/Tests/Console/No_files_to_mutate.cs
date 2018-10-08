using System.Linq;
using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class No_files_to_mutate : Contexts.Default
    {
        public No_files_to_mutate()
        {
            Given_a_config_file_where_all_source_files_are_filtered_out();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_a_warning_is_output()
        {
            Assert.That(SpyOutputWriter.WrittenNormalLines, Has.Count.GreaterThan(0));
            Assert.That(SpyOutputWriter.WrittenNormalLines.SingleOrDefault(x => 
                    x.Contains("No source files found to mutate")), 
                Is.Not.Null);
        }

        [Test]
        public void Then_coverage_analysis_is_not_attempted()
        {
            MockCoverageAnalyser.Verify(
                mtr => mtr.AnalyseCoverage(It.IsAny<Config>()),
                Times.Never);
        }

        [Test]
        public void Then_mutation_testing_is_not_attempted()
        {
            MockMutationTestRunner.Verify(
                mtr => mtr.Run(It.IsAny<Config>()),
                Times.Never);
        }

        [Test]
        public void Then_the_exit_code_still_indicates_success()
        {
            Assert.That(ExitCode, Is.EqualTo(0));
        }
    }
}