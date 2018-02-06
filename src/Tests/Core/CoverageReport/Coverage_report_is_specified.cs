using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.CoverageReport
{
    [TestFixture]
    class Coverage_report_is_specified : Contexts.Default
    {
        public Coverage_report_is_specified()
        {
            Given_an_app_to_be_mutation_tested();
            Given_a_coverage_report("valid-opencover.xml");

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_methods_marked_as_covered_by_tests_are_mutated()
        {
            Assert.That(SpyEventListener.BegunMethods.Contains(
                    "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)"),
                Is.True);
        }

        [Test]
        public void Then_methods_marked_as_not_covered_by_tests_are_not_mutated()
        {
            Assert.That(SpyEventListener.BegunMethods.Contains(
                    "System.Boolean HasSurvivingMutants.Implementation.UntestedNumberComparison::IsMeaningful(System.Int32)"),
                Is.False);
        }
        
        [Test]
        public void Then_methods_not_present_in_the_report_at_all_are_not_mutated()
        {
            var partialMethodNames = new []
            {
                "IsNegative(System.Int32)",
                "IsZero(System.Int32)",
                "AreBothZero(int,int)",
                "Sum(System.Int32,System.Int32)",
                "Preincrement(System.Int32)",
                "Postincrement(System.Int32)"
            };
            foreach (var partialMethodName in partialMethodNames)
            {
                Assert.That(SpyEventListener.BegunMethods.Any(mn => mn.EndsWith(partialMethodName)), Is.False);
            }
        }

    }
}
