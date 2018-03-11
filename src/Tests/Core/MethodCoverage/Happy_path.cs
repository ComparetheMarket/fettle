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
            Given_project_filters("HasSurvivingMutants.Implementation", "HasSurvivingMutants.MoreImplementation");

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
        public void Then_methods_are_covered_by_the_test_cases_that_call_them_during_testing_even_if_they_are_empty()
        {
            const string methodName = "System.Void HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::EmptyMethod()";
            var coveringTests = Result.MethodsAndTheirCoveringTests[methodName];
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.EmptyMethod",
            }));
        }

        [Test]
        public void Then_methods_are_considered_to_be_covered_by_test_cases_if_their_fixture_setup_or_teardown_methods_call_them()
        {
            const string methodName = "System.Boolean HasSurvivingMutants.MoreImplementation.CalledByTestFixture::IsFourtyTwo(System.Int32)";
            var coveringTests = Result.MethodsAndTheirCoveringTests[methodName];
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.CalledByTestFixture_Constructor.TestCase",
                "HasSurvivingMutants.Tests.CalledByTestFixture_OneTimeSetup.TestCase",
                "HasSurvivingMutants.Tests.CalledByTestFixture_Setup.TestCase",
                "HasSurvivingMutants.Tests.CalledByTestFixture_Teardown.TestCase",
                "HasSurvivingMutants.Tests.CalledByTestFixture_OneTimeTeardown.TestCase"
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
                .All(method => method.Contains("HasSurvivingMutants.Implementation") ||
                               method.Contains("HasSurvivingMutants.MoreImplementation"));

            Assert.That(onlyContainsMethodsThatMatchFilter, Is.True,
                $"Actual keys: {string.Join(Environment.NewLine, Result.MethodsAndTheirCoveringTests.Keys)}");
        }
    }
}