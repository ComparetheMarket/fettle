using System.Collections.Generic;

namespace Fettle.Core.Internal
{
    internal interface ITestRunner
    {
        TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames);
    }
}