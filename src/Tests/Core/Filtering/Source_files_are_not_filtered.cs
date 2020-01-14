using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.Filtering
{
    class Source_files_are_not_filtered : Contexts.Default
    {
        public Source_files_are_not_filtered()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();            
            Given_project_filters(new string[0]);
            
            Given_source_file_filters(new string[0]);

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_all_files_are_candidates_for_mutation()
        {
            Assert.That(SpyEventListener.BegunFiles.Any(f => f.EndsWith(@"Implementation\PartiallyTestedNumberComparison.cs")));
            Assert.That(SpyEventListener.BegunFiles.Any(f => f.EndsWith(@"Implementation\MorePartiallyTestedNumberComparison.cs")));            
        }
    }
}
