using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Multiple_mutants_per_syntax_node : Contexts.Default
    {
        public Multiple_mutants_per_syntax_node()
        {
            Given_a_partially_tested_app_in_which_multiple_mutants_survive_for_a_syntax_node();
            Given_source_file_filters("**/PartiallyTestedNumberComparison.cs");

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_only_one_surviving_mutant_is_returned_per_syntax_node()
        {
            Assert.That(Result.SurvivingMutants.Count(sm => sm.SourceLine == 7), Is.EqualTo(1));
        }
    }
}