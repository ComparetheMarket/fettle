using System.IO;

namespace Fettle.Core.Internal
{
    internal static class DirectoryUtils
    {        
        public static void CopyDirectoryContents(string binariesDirectory, string tempDirectory)
        {
            var sourcePath = Path.GetFullPath(binariesDirectory) + Path.DirectorySeparatorChar;
            var destinationPath = Path.GetFullPath(tempDirectory) + Path.DirectorySeparatorChar;

            foreach (string subDir in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(subDir.Replace(sourcePath, destinationPath));
            }

            foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                File.Copy(file, file.Replace(sourcePath, destinationPath));
            }
        }
    }
}