using System.Collections.Generic;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitTestFinder : NUnitTestEngine, ITestFinder
    {
        public string[] FindTests(IEnumerable<string> testAssemblyFilePaths) => FindTestsInAssemblies(testAssemblyFilePaths);
    }
}