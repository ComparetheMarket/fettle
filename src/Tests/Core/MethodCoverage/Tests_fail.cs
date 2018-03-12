using NUnit.Framework;

namespace Fettle.Tests.Core.MethodCoverage
{
    class Tests_fail : Contexts.Coverage
    {
        public Tests_fail()
        {
            Given_an_app_with_tests();
            Given_project_filters("HasSurvivingMutants.Implementation");

            Given_some_failing_tests();

            When_analysing_method_coverage();
        }

        [Test]
        public void Then_analysis_fails()
        {
            Assert.That(Result.ErrorDescription, Is.Not.Null);
        }

        [Test]
        public void Then_error_description_indicates_the_test_that_failed()
        {
            Assert.That(Result.ErrorDescription, 
                Does.Contain("HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred"));
        }
    }
}