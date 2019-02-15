using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.EndToEnd
{
    class Happy_path_mutating_custom_test_runner : Contexts.EndToEnd
    {
        public Happy_path_mutating_custom_test_runner()
        {
            Given_an_app_which_has_a_custom_test_runner();

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_the_expected_surviving_mutants_are_returned()
        {
            Assert.That(MutationTestResult.SurvivingMutants.Count, Is.EqualTo(1),
                string.Join(Environment.NewLine, MutationTestResult.SurvivingMutants.Select(sm =>
                    $"{Path.GetFileName(sm.SourceFilePath)}:{sm.SourceLine} \"{sm.OriginalLine}\" => \"{sm.MutatedLine}\"")));
        }
    }
}
