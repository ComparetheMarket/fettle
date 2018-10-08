using System;
using System.IO;
using System.Linq;
using Fettle.Core;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class GitIntegration_Tests
    {
        [TestFixture]
        class Modifications_exist : Contexts.Default
        {
            private string[] tempFiles;

            [SetUp]
            public void SetUp()
            {
                var baseDir = Path.GetDirectoryName(Config.SolutionFilePath);
                tempFiles = new[]
                {
                    Path.Combine(baseDir, "Implementation", "Temp.csproj"),
                    Path.Combine(baseDir, "Implementation", "TempSourceFile.cs"),
                    Path.Combine(baseDir, "Implementation", "OtherTempSourceFile.CS"),
                    Path.Combine(baseDir, "Implementation", "Temp.txt")
                };
                tempFiles.ToList().ForEach(f => File.WriteAllText(f, "dummy file contents"));
            }

            [TearDown]
            public void TearDown()
            {
                tempFiles.ToList().ForEach(f => File.Delete(f));
            }

            [Test]
            public void When_locally_modified_csharp_files_exist_they_are_returned_relative_to_the_directory_of_the_solution_file()
            {
                var integration = new GitIntegration();

                var modifiedFiles = integration.FindLocallyModifiedFiles(Config);

                Assert.That(modifiedFiles, Is.EquivalentTo(new[] 
                {
                    @"Implementation\TempSourceFile.cs",
                    @"Implementation\OtherTempSourceFile.CS"
                }));
            }
        }

        [TestFixture]
        class No_modification_exist : Contexts.Default
        {
            [Test]
            public void When_locally_modified_files_do_not_exist_an_empty_array_is_returned()
            {
                var integration = new GitIntegration();

                var modifiedFiles = integration.FindLocallyModifiedFiles(Config);

                Assert.That(modifiedFiles, Is.Empty);
            }
        }

        [TestFixture]
        class Unable_to_find_modifications : Contexts.Default
        {
            [Test]
            public void When_unable_to_find_local_modifications_then_an_exception_is_thrown()
            {
                // Cause "git status" to fail by pointing to a .sln file that's not within a git repo.
                var solutionFileNotWithinGitRepo = Path.Combine(Path.GetDirectoryName(Config.SolutionFilePath), 
                    "..", "..", "..", "..", "Example.sln");

                Config.SolutionFilePath = solutionFileNotWithinGitRepo;

                var integration = new GitIntegration();

                Assert.Throws<SourceControlIntegrationException>(() => integration.FindLocallyModifiedFiles(Config));
            }
        }
    }
}

