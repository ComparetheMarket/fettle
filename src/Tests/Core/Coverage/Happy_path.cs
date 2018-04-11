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
            var coveringTests = Result.TestsThatCoverMethod(methodName, Config.TestAssemblyFilePaths.First());
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
            foreach (var testAssemblyFilePath in Config.TestAssemblyFilePaths)
            {
                var coveringTests = Result.TestsThatCoverMethod(methodName, testAssemblyFilePath);
                Assert.That(coveringTests, Has.Length.Zero);    
            }
        }

        [Test]
        public void Then_methods_are_covered_by_the_test_cases_that_call_them_during_testing_even_if_they_are_empty()
        {
            const string methodName = "System.Void HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::EmptyMethod()";
            var coveringTests = Result.TestsThatCoverMethod(methodName, Config.TestAssemblyFilePaths.First());
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.EmptyMethod",
            }));
        }

        [Test]
        public void Then_methods_are_considered_to_be_covered_by_test_cases_if_their_fixture_setup_or_teardown_methods_call_them()
        {
            const string methodName = "System.Boolean HasSurvivingMutants.MoreImplementation.CalledByTestFixture::IsFourtyTwo(System.Int32)";
            var coveringTests = Result.TestsThatCoverMethod(methodName, Config.TestAssemblyFilePaths.First());            
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
        public void Then_methods_can_be_covered_by_tests_from_multiple_projects()
        {
            const string methodName = "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Postincrement(System.Int32)";
            var coveringTests1 = Result.TestsThatCoverMethod(methodName, Config.TestAssemblyFilePaths[0]);            
            Assert.That(coveringTests1, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.Postincrement",
            }));
            var coveringTests2 = Result.TestsThatCoverMethod(methodName, Config.TestAssemblyFilePaths[1]);
            Assert.That(coveringTests2, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.MoreTests.MoreTests.PostIncrement2"
            }));
        }
        
        [Test]
        public void Then_methods_can_be_covered_by_parameterised_tests()
        {
            const string methodName = "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Preincrement(System.Int32)";
            var coveringTests1 = Result.TestsThatCoverMethod(methodName, Config.TestAssemblyFilePaths[0]);            
            Assert.That(coveringTests1, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.Preincrement",
            }));
            var coveringTests2 = Result.TestsThatCoverMethod(methodName, Config.TestAssemblyFilePaths[1]);
            Assert.That(coveringTests2, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.MoreTests.MoreTests.PreIncrement2(1,2)",
                "HasSurvivingMutants.MoreTests.MoreTests.PreIncrement2(4,5)",
            }));
        }

        [Test]
        public void Then_methods_that_throw_an_exception_can_be_covered_by_tests()
        {
            const string methodName = "System.Void HasSurvivingMutants.Implementation.OtherMethods::ThrowingMethod()";
            var coveringTests = Result.TestsThatCoverMethod(methodName, Config.TestAssemblyFilePaths[1]);
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.MoreTests.MoreTests.ThrowingMethod"
            }));
        }

        [Test]
        public void Then_only_methods_that_match_the_configured_project_filters_are_analysed()
        {
            var onlyContainsMethodsThatMatchFilter = Result.AllAnalysedMethods 
                .All(method => method.Contains("HasSurvivingMutants.Implementation") ||
                               method.Contains("HasSurvivingMutants.MoreImplementation"));

            Assert.That(onlyContainsMethodsThatMatchFilter, Is.True,
                $"Actual keys: {string.Join(Environment.NewLine, Result.AllAnalysedMethods)}");
        }

        [Test]
        public void Then_events_are_raised_when_the_analysis_of_tests_begins()
        {
            MockEventListener.Verify(
                el => el.BeginCoverageAnalysisOfTestCase(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.AtLeastOnce);
        }

        [Test]
        public void Then_abstract_methods_are_ignored()
        {
            const string methodName = "System.Void HasSurvivingMutants.Implementation.SkipAbstractMethodMutation::BuildSomething()";
            Assert.That(Result.IsMethodCovered(methodName), Is.False);
        }
    }
}