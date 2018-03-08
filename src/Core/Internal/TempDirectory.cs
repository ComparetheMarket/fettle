using System;
using System.IO;

namespace Fettle.Core.Internal
{
    public class TempDirectory
    {
        public static string Create()
        {
            var path = Path.Combine(Path.GetTempPath(), $"fettle-{Guid.NewGuid()}");
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
