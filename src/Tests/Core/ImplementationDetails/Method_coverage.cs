using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class Method_coverage : Contexts.Coverage
    {
        public Method_coverage()
        {
            Given_an_app_with_tests();
            When_analysing_method_coverage();
        }

        [Test]
        public void Then_methods_are_covered_by_the_tests_that_call_them_during_testing()
        {
            var coveringTests = MethodCoverage.TestsThatCoverMethod(
                "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)");

            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsPositive",
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.PositiveOrNegative"
            }));
        }

        [Test]
        public void Then_methods_that_are_not_called_are_not_covered_by_any_tests()
        {
            var coveringTests = MethodCoverage.TestsThatCoverMethod(
                "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::AddNumbers_should_be_ignored(System.Int32)");

            Assert.That(coveringTests, Has.Length.Zero);
        }
    }
}
