using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Invalid_mutation : Contexts.Default
    {
        public Invalid_mutation()
        {
            Given_an_app_to_be_mutation_tested();

            Given_a_single_file_is_mutated("ProducesInvalidMutation.cs");

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_the_mutation_is_skipped()
        {
            Assert.That(SpyEventListener.HaveAnyMutantsBeenSkipped, Is.True);

            Assert.That(SpyEventListener.HaveAnyMutantsBeenKilled, Is.False);
            Assert.That(SpyEventListener.HaveAnyMutantsSurvived, Is.False);
        }

        [Test]
        public void Then_mutation_testing_does_not_fail()
        {
            Assert.That(MutationTestResult.Errors, Has.Count.Zero);
        }
    }
}