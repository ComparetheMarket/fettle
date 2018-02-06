using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Ignored_lines : Contexts.Default
    {
        public Ignored_lines()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();
            
            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_lines_marked_as_ignore_are_not_mutation_tested()
        {
            Assert.That(SpyEventListener.BegunMethods.Any(m => m.Contains("AddNumbers_should_be_ignored")), Is.False);
        }
    }
}