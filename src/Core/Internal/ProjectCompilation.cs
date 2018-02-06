using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Fettle.Core.Internal
{
    internal static class ProjectCompilation
    {
        public static EmitResult CompileProject(string outputFilePath, Compilation compilation)
        {            
            using (var dllFileStream = new FileStream(outputFilePath, FileMode.Create))
            {
                return compilation.Emit(
                    dllFileStream,
                    win32Resources: compilation.CreateDefaultWin32Resources(
                        versionResource: true,
                        noManifest: false,
                        manifestContents: Stream.Null,
                        iconInIcoFormat: null));
            }
        }
    }
}