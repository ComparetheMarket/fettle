using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.CoverageReport
{
    class Coverage_report_is_not_specified : Contexts.Default
    {
        public Coverage_report_is_not_specified()
        {
            Given_an_app_to_be_mutation_tested();
            Given_a_coverage_report(null);

            When_mutation_testing_the_app();
        }
        
        [Test]
        public void Then_methods_are_mutated_whether_covered_or_not()
        {
            Assert.That(SpyEventListener.BegunMethods.Contains(
                    "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)"),
                Is.True);

            Assert.That(SpyEventListener.BegunMethods.Contains(
                    "System.Boolean HasSurvivingMutants.Implementation.UntestedNumberComparison::IsMeaningful(System.Int32)"),
                Is.True);
        }
    }
}