using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Invalid_mutation : Contexts.Default
    {
        public Invalid_mutation()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();

            Given_source_file_filters("*ProducesInvalidMutation.cs");

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_testing_is_not_attempted_because_compilation_fails()
        {
            Assert.That(SpyEventListener.HaveAnyFilesBegun, Is.False);
        }
    }
}