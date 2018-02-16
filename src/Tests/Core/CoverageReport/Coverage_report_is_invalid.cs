using NUnit.Framework;

namespace Fettle.Tests.Core.CoverageReport
{
    class No_methods_are_covered : Contexts.Default
    {
        public No_methods_are_covered()
        {
            Given_an_app_to_be_mutation_tested();
            Given_a_coverage_report("nomethods-opencover.xml");

            When_mutation_testing_the_app(captureException: true);
        }

        [Test]
        public void Then_an_error_is_returned()
        {
            Assert.That(Result.Errors, Is.Not.Null);
            Assert.That(Result.Errors.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Then_mutation_is_not_attempted()
        {
            Assert.That(SpyEventListener.BegunFiles, Is.Empty);
        }
    }
}