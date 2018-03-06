using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Mutants_survive : Contexts.Default
    {
        public Mutants_survive()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();
            Given_source_file_filters("**/PartiallyTestedNumberComparison.cs");

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_the_surviving_mutant_is_returned()
        {
            Assert.That(MutationTestResult.SurvivingMutants.Count, Is.EqualTo(1));
        }

        [Test]
        public void Then_the_location_of_the_surviving_mutant_is_returned()
        {
            var survivor = MutationTestResult.SurvivingMutants.Single();
            Assert.That(survivor.SourceFilePath, Does.EndWith("PartiallyTestedNumberComparison.cs"));
            Assert.That(survivor.SourceLine, Is.EqualTo(7));
        }

        [Test]
        public void Then_the_relevant_pre_mutation_source_code_is_returned()
        {
            Assert.That(MutationTestResult.SurvivingMutants.Single().OriginalLine, Does.EndWith("return a > 0;"));
        }

        [Test]
        public void Then_the_relevant_post_mutation_source_code_is_returned()
        {
            Assert.That(MutationTestResult.SurvivingMutants.Single().MutatedLine, Does.EndWith("return a < 0;"));
        }
    }
}
