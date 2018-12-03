using NUnit.Framework;

namespace Fettle.Tests.Core.Coverage
{
    class Tests_included_by_test_project_filters : Contexts.Coverage
    {
        public Tests_included_by_test_project_filters()
        {
            Given_an_app_with_tests();
            Given_test_project_filters("HasSurvivingMutants.Tests");

            When_analysing_coverage();
        }

        [Test]
        public void Then_tests_within_included_test_projects_are_run()
        {
            const string memberName = "System.Void HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)";

            var coveringTests = Result.TestsThatCoverMember(memberName, "HasSurvivingMutants.Tests");
            Assert.That(coveringTests, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsPositive",
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.PositiveOrNegative",
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.PositiveOrNegativeAsExpressionBody"
            }));
        }
    }

    class Tests_excluded_by_test_project_filters : Contexts.Coverage
    {
        public Tests_excluded_by_test_project_filters()
        {
            Given_an_app_with_tests();
            Given_test_project_filters("HasSurvivingMutants.MoreTests");

            When_analysing_coverage();
        }

        [Test]
        public void Then_tests_within_excluded_test_projects_are_not_run()
        {
            const string memberName = "System.Void HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)";

            var coveringTests = Result.TestsThatCoverMember(memberName, "HasSurvivingMutants.Tests");
            Assert.That(coveringTests, Has.Length.Zero);
        }
    }
}