using System.Collections.Generic;

namespace Fettle.Core.Internal
{
    internal interface ITestFinder
    {
        string[] FindTests(IEnumerable<string> testAssemblyFilePaths);
    }
}