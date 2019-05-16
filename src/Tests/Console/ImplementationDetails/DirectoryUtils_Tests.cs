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
            var path = Path.GetDirectoryName("FilenameWithoutPath.sln");

            Assert.That(() => DirectoryUtils.SafeGetFullPath(path), Throws.Nothing);
        }

        [Test]
        public void SafeGetFullPath_current_directory_is_used_in_result_when_argument_contains_no_folder_info()
        {
            const string path = "FilenameWithoutPath.sln";

            var actual = DirectoryUtils.SafeGetFullPath(path);

            Assert.That(actual, Is.EqualTo(Path.Combine(Directory.GetCurrentDirectory(), path)));
        }

        [Test]
        public void SafeGetFullPath_returns_correct_resul_when_argument_contains_no_folder_info()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = Directory.GetFiles(currentDirectory).First();
            var filenameWithoutPath = Path.GetFileName(path);

            var actual = DirectoryUtils.SafeGetFullPath(filenameWithoutPath);

            Assert.That(actual, Is.EqualTo(path));
        }
    }
}
