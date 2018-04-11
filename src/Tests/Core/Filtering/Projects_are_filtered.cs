using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.Filtering
{
    class Projects_are_filtered : Contexts.Default
    {
        public Projects_are_filtered()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();
            Given_source_file_filters(null);

            Given_project_filters("HasSurvivingMutants.MoreImplementation");

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_only_files_matching_filter_are_candidates_for_mutation()
        {
            Assert.That(SpyEventListener.BegunFiles.All(f => f.Contains(@"\MoreImplementation\")));
        }
    }
}