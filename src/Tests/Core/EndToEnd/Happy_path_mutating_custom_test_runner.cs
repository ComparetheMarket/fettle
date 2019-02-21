using System;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.EndToEnd
{
    class Happy_path_mutating_custom_test_runner : Contexts.EndToEnd
    {
        public Happy_path_mutating_custom_test_runner()
        {
            Given_an_app_which_has_a_custom_test_runner();

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_mutation_testing_is_successful()
        {
            Assert.That(MutationTestResult.Errors, Is.Empty);
        }

        [Test]
        public void Then_the_expected_surviving_mutants_are_returned()
        {
            Assert.That(MutationTestResult.SurvivingMutants.Count, Is.EqualTo(1));
            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                    sm => sm.SourceFilePath.EndsWith("MorePartiallyTestedNumberComparison.cs") &&
                          sm.SourceLine == 7 &&
                          sm.OriginalLine.EndsWith("return n > 100;") &&
                          sm.MutatedLine.EndsWith("return n >= 100;")),
                Is.Not.Null);
        }

        [Test]
        public void Then_events_raised_when_mutations_fail_include_a_description_of_the_test_failure()
        {
            Assert.That(SpyEventListener.KilledMutantsAndTheirTestFailures.All(x => !string.IsNullOrEmpty(x.Item2)), Is.True,
                message: $"actual: {string.Join(Environment.NewLine, SpyEventListener.KilledMutantsAndTheirTestFailures.Select(x => x.Item2))}");
        }
    }
}
