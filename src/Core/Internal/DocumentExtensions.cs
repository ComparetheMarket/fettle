using System.IO;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{
    internal static class DocumentExtensions
    {
        public static bool IsAutomaticallyGenerated(this Document document)
        {
            if (document.FilePath.StartsWith(Path.GetTempPath())) return true;

            return false;
        }
    }
}
