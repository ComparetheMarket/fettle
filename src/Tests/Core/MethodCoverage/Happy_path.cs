using System;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.MethodCoverage
{
    class Happy_path : Contexts.Coverage
    {
        public Happy_path()
        {
            Given_an_app_with_tests();
            Given_project_filters("HasSurvivingMutants.Implementation");

            When_analysing_method_coverage();
        }

        [Test]
        public void Then_analysis_is_successful()
        {
            Assert.That(Result.ErrorDescription, Is.Null);
        }

        [Test]
        public void Then_methods_are_covered_by_the_tests_that_call_them_during_testing()
        {
            const string methodName = "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)";
            var coveringTests = Result.MethodsAndTheirCoveringTests[methodName];
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsPositive",
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.PositiveOrNegative"
            }));
        }

        [Test]
        public void Then_methods_that_are_not_called_are_recognised_as_not_covered_by_any_tests()
        {
            const string methodName = "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::AddNumbers_should_be_ignored(System.Int32)";
            Assert.That(Result.MethodsAndTheirCoveringTests.ContainsKey(methodName), Is.False);
        }

        [Test]
        public void Then_only_methods_that_match_the_configured_project_filters_are_analysed()
        {
            var onlyContainsMethodsThatMatchFilter = Result.MethodsAndTheirCoveringTests
                .Keys
                .All(method => method.Contains("HasSurvivingMutants.Implementation"));

            Assert.That(onlyContainsMethodsThatMatchFilter, Is.True,
                $"Actual keys: {string.Join(Environment.NewLine, Result.MethodsAndTheirCoveringTests.Keys)}");
        }
    }
}