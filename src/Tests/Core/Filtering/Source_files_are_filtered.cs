using System.Linq;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Core.Filtering
{
    class Source_files_are_filtered : Contexts.Default
    {
        public Source_files_are_filtered()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();
            Given_project_filters(null);

            Given_source_file_filters(@"**\*Comparison.cs");

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_only_files_matching_filter_are_candidates_for_mutation()
        {
            Assert.That(SpyEventListener.BegunFiles.All(f => f.EndsWith("Comparison.cs")));            
        }
    }
}
