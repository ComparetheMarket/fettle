using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class No_mutants_survive : Contexts.Default
    {
        public No_mutants_survive()
        {
            Given_a_fully_tested_app_in_which_no_mutants_will_survive();

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_no_surviving_mutants_are_returned()
        {
            Assert.That(Result.SurvivingMutants.Count, Is.EqualTo(0));
        }
    }
}