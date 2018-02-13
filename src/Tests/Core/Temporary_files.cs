using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Temporary_files : Contexts.Default
    {
        public Temporary_files()
        {
            Given_an_app_to_be_mutation_tested();
            Given_there_are_no_pre_existing_temporary_files();

            When_mutation_testing_the_app();
        }
        
        [Test]
        public void Then_temporary_files_do_not_remain_after_mutation_testing_is_complete()
        {            
            Assert.That(TempDirectories, Has.Length.Zero);
        }
    }
}