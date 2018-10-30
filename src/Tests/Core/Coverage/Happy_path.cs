using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Core.Coverage
{
    class Happy_path : Contexts.Coverage
    {
        public Happy_path()
        {
            Given_an_app_with_tests();
            Given_project_filters("HasSurvivingMutants.Implementation", "HasSurvivingMutants.MoreImplementation");
            Given_test_project_filters("HasSurvivingMutants.Tests", "HasSurvivingMutants.MoreTests");

            When_analysing_coverage();
        }

        [Test]
        public void Then_analysis_is_successful()
        {
            Assert.That(Result.ErrorDescription, Is.Null);
        }

        [Test]
        public void Then_members_are_covered_by_the_tests_that_call_them_during_testing()
        {
            const string memberName = "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)";
            var coveringTests = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters.First());
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsPositive",
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.PositiveOrNegative",
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.PositiveOrNegativeAsExpressionBody"
            }));
        }

        [Test]
        public void Then_members_that_are_not_called_are_recognised_as_not_covered_by_any_tests()
        {
            const string memberName = "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::AddNumbers_should_be_ignored(System.Int32)";
            foreach (var testAssemblyFilePath in Config.TestProjectFilters)
            {
                var coveringTests = Result.TestsThatCoverMember(memberName, testAssemblyFilePath);
                Assert.That(coveringTests, Has.Length.Zero);
            }
        }

        [Test]
        public void Then_members_are_covered_by_the_test_cases_that_call_them_during_testing_even_if_they_are_empty()
        {
            const string memberName = "System.Void HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::EmptyMethod()";
            var coveringTests = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters.First());
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.EmptyMethod",
            }));
        }

        [Test]
        public void Then_members_are_considered_to_be_covered_by_test_cases_if_their_fixture_setup_or_teardown_methods_call_them()
        {
            const string memberName = "System.Boolean HasSurvivingMutants.MoreImplementation.CalledByTestFixture::IsFourtyTwo(System.Int32)";
            var coveringTests = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters.First());            
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
        public void Then_members_can_be_covered_by_tests_from_multiple_projects()
        {
            const string memberName = "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Postincrement(System.Int32)";
            var coveringTests1 = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters[0]);            
            Assert.That(coveringTests1, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.Postincrement",
            }));
            var coveringTests2 = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters[1]);
            Assert.That(coveringTests2, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.MoreTests.MoreTests.PostIncrement2"
            }));
        }

        [Test]
        public void Then_members_that_are_properties_can_be_covered_by_tests()
        {
            const string memberName = "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IntegerProperty";
            var coveringTests = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters[0]);
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.IntegerProperty",
            }));
        }

        [Test]
        public void Then_members_can_be_covered_by_parameterised_tests()
        {
            const string memberName = "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Preincrement(System.Int32)";
            var coveringTests1 = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters[0]);            
            Assert.That(coveringTests1, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.Preincrement",
            }));
            var coveringTests2 = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters[1]);
            Assert.That(coveringTests2, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.MoreTests.MoreTests.PreIncrement2"
            }));
        }

        [Test]
        public void Then_members_can_be_covered_by_tests_with_test_case_sources()
        {
            const string memberName = "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Sum(System.Int32,System.Int32)";
            var coveringTests = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters[1]);            
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.MoreTests.MoreTests.Sum2"
            }));
        }

        [Test]
        public void Then_members_that_throw_an_exception_can_be_covered_by_tests()
        {
            const string memberName = "System.Void HasSurvivingMutants.Implementation.OtherMethods::ThrowingMethod()";
            var coveringTests = Result.TestsThatCoverMember(memberName, Config.TestProjectFilters[1]);
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.MoreTests.MoreTests.ThrowingMethod"
            }));
        }

        [Test]
        public void Then_only_members_that_match_the_configured_project_filters_are_analysed()
        {
            var onlyContainsMembersThatMatchFilter = Result.AllAnalysedMembers 
                .All(method => method.Contains("HasSurvivingMutants.Implementation") ||
                               method.Contains("HasSurvivingMutants.MoreImplementation"));

            Assert.That(onlyContainsMembersThatMatchFilter, Is.True,
                $"Actual keys: {string.Join(Environment.NewLine, Result.AllAnalysedMembers)}");
        }

        [Test]
        public void Then_events_are_raised_when_the_analysis_of_tests_begins()
        {
            MockEventListener.Verify(
                el => el.BeginCoverageAnalysisOfTestCase(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.AtLeastOnce);
        }
        
        [Test]
        public void Then_members_with_no_bodies_are_ignored()
        {
            const string memberName = "System.Void HasSurvivingMutants.Implementation.AbstractClass::AbstractMethod()";
            Assert.That(Result.IsMemberCovered(memberName), Is.False);
        }
    }
}