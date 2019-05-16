using System.IO;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{
    internal static class DocumentExtensions
    {
        public static bool IsAutomaticallyGenerated(this Document document)
        {
            return document.FilePath.StartsWith(Path.GetTempPath());
        }
    }
}
