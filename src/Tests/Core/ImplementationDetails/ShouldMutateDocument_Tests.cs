using Fettle.Core;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class ShouldMutateDocument_Tests
    {
        [TestCase(@"c:\blah\src\SomeFile.cs",  @"**\Other*",  @"src\OtherFile.cs",  false)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  @"**\Some*",   @"src\OtherFile.cs",  false)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  null,          @"src\OtherFile.cs",  false)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  @"**\Other*",  @"src\SomeFile.cs",   false)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  @"**\Other*",  "",                   false)] 
        [TestCase(@"c:\blah\src\SomeFile.cs",  @"**\Other*",  null,                 false)] 
        [TestCase(@"c:\blah\src\SomeFile.cs",  @"**\Some*",   @"src\SomeFile.cs",   true)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  @"**\Some*",   "",                   false)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  @"**\Some*",   null,                 true)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  null,          @"src\SomeFile.cs",   true)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  null,          "",                   false)]
        [TestCase(@"c:\blah\src\SomeFile.cs",  null,          null,                 true)]
        public void A_source_file_can_be_skipped_via_filters_or_local_modification_list(
            string sourceFilePath,
            string sourceFileFilters, 
            string modifications,
            bool expectedToBeMutatable)
        {
            var config = new Config
            {
                SolutionFilePath = @"c:\blah\SomeApp.sln",
                SourceFileFilters = sourceFileFilters == null ? new string[0] : new[]{ sourceFileFilters },
                LocallyModifiedSourceFiles = modifications == null ? null :
                    (modifications == "" ? new string[0] : new[]{ modifications })
            };
            var document = CreateDocument(sourceFilePath);

            var isMutatable = Fettle.Core.Internal.Filtering.ShouldMutateDocument(document, config);

            Assert.That(isMutatable, Is.EqualTo(expectedToBeMutatable));
        }

        private static Document CreateDocument(string documentFilePath)
        {
            using (var ws = new AdhocWorkspace())
            {
                var emptyProject = ws.AddProject(
                    ProjectInfo.Create(
                        ProjectId.CreateNewId(),
                        VersionStamp.Default,
                        "test",
                        "test.dll",
                        LanguageNames.CSharp));

                return emptyProject.AddDocument("SomeFile", "", filePath: documentFilePath);
            }
        }
    }
}
