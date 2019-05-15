using System.IO;
using System.Linq;
using Fettle.Core.Internal;
using NUnit.Framework;

namespace Fettle.Tests.Console.ImplementationDetails
{
    [TestFixture]
    public class DirectoryUtils_Tests
    {
        [Test]
        public void SafeGetFullPath_does_not_throw_exceptions_when_argument_contains_no_folder_info()
        {
            // Arrange
            var path = Path.GetDirectoryName("FilenameWithoutPath.sln");

            // Act & Assert
            Assert.That(() => DirectoryUtils.SafeGetFullPath(path), Throws.Nothing);
        }

        [Test]
        public void SafeGetFullPath_current_directory_is_used_in_result_when_argument_contains_no_folder_info()
        {
            // Arrange
            const string path = "FilenameWithoutPath.sln";

            // Act 
            var actual = DirectoryUtils.SafeGetFullPath(path);

            // Assert
            Assert.That(actual, Is.EqualTo(Path.Combine(Directory.GetCurrentDirectory(), path)));
        }

        [Test]
        public void SafeGetFullPath_returns_correct_resul_when_argument_contains_no_folder_info()
        {
            // Arrange
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = Directory.GetFiles(currentDirectory).First();
            var filenameWithoutPath = Path.GetFileName(path);

            // Act 
            var actual = DirectoryUtils.SafeGetFullPath(filenameWithoutPath);

            // Assert
            Assert.That(actual, Is.EqualTo(path));
        }
    }
}
