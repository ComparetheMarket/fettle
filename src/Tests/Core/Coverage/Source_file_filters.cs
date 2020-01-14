using NUnit.Framework;

namespace Fettle.Tests.Core.Coverage
{
    class Source_file_filters : Contexts.Coverage
    {
        public Source_file_filters()
        {
            Given_an_app_with_tests();
            Given_project_filters("HasSurvivingMutants.Implementation", "HasSurvivingMutants.MoreImplementation");

            Given_source_file_filters(@"Implementation\OtherMethods.cs");

            When_analysing_coverage();
        }
        
        [Test]
        public void Then_analysis_is_successful()
        {
            Assert.That(Result.ErrorDescription, Is.Null);
        }

        [Test]
        public void Then_members_are_covered_by_the_tests_that_call_them_during_testing()
        {
            const string memberName = "System.Void HasSurvivingMutants.Implementation.OtherMethods::ThrowingMethod()";
            
            Assert.That(Config.TestAssemblyFilePaths, Has.Length.EqualTo(2));
            Assert.That(Result.TestsThatCoverMember(memberName, Config.TestAssemblyFilePaths[0]), Has.Length.Zero);
            Assert.That(Result.TestsThatCoverMember(memberName, Config.TestAssemblyFilePaths[1]), Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.MoreTests.MoreTests.ThrowingMethod"
            }));
        }

        [Test]
        public void Then_only_files_that_match_the_filters_are_analysed()
        {
            Assert.That(Result.AllAnalysedMembers, Is.EquivalentTo(new[]
            {
                "System.Void HasSurvivingMutants.Implementation.OtherMethods::ThrowingMethod()"
            }));
        }
    }
}