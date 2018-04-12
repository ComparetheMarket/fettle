using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.Filtering
{
    class Projects_are_filtered_with_multiple_filters : Contexts.Default
    {
        public Projects_are_filtered_with_multiple_filters()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();
            Given_source_file_filters(null);

            Given_project_filters("HasSurvivingMutants.Implementation", "HasSurvivingMutants.MoreImplementation");

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_only_files_matching_either_filter_are_candidates_for_mutation()
        {
            Assert.That(SpyEventListener.BegunFiles.Any());
            Assert.That(SpyEventListener.BegunFiles.All(f => f.Contains(@"\MoreImplementation\") ||
                                                             f.Contains(@"\Implementation\")));
        }
    }
}