using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Events : Contexts.Default
    {
        public Events()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_event_listener_is_called_when_events_occur()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SpyEventListener.HaveAnyFilesBegun);
                Assert.That(SpyEventListener.HaveAnyMembersBegun);
                Assert.That(SpyEventListener.HaveAnySyntaxNodesBegun);
                Assert.That(SpyEventListener.HaveAnyFilesEnded);
                Assert.That(SpyEventListener.HaveAnyMutantsSurvived);
                Assert.That(SpyEventListener.HaveAnyMutantsBeenKilled);
            });
        }

        [Test]
        public void Then_each_file_is_reported_as_begun_once_only()
        {
            Assert.That(SpyEventListener.BegunFiles, Is.EquivalentTo(SpyEventListener.BegunFiles.Distinct()));
        }

        [Test]
        public void Then_each_member_is_reported_as_begin_once_only()
        {
            Assert.That(SpyEventListener.BegunMembers, Is.EquivalentTo(SpyEventListener.BegunMembers.Distinct()));
        }
    }

    class Events_relating_to_skipped_mutations : Contexts.Default
    {
        public Events_relating_to_skipped_mutations()
        {
            Given_some_methods_are_not_covered_by_tests();
            Given_an_app_to_be_mutation_tested();

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_event_listener_is_called_when_mutation_of_uncovered_code_is_skipped()
        {
            Assert.That(SpyEventListener.HaveAnyMutantsBeenSkipped);
        }
    }
}
