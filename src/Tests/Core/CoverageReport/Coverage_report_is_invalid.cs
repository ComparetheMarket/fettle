using NUnit.Framework;

namespace Fettle.Tests.Core.CoverageReport
{
    [TestFixture("invalidxml-opencover.xml")]
    [TestFixture("nomethods-opencover.xml")]
    class Coverage_report_is_invalid : Contexts.Default
    {
        public Coverage_report_is_invalid(string coverageReportFilename)
        {
            Given_an_app_to_be_mutation_tested();
            Given_a_coverage_report(coverageReportFilename);

            When_mutation_testing_the_app(captureException: true);
        }

        [Test]
        public void Then_an_exception_is_thrown()
        {
            Assert.That(Exception, Is.Not.Null);
        }

        [Test]
        public void Then_mutation_is_not_attempted()
        {
            Assert.That(SpyEventListener.BegunFiles, Is.Empty);
        }
    }
}