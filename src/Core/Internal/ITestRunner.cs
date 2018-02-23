using System.Collections.Generic;

namespace Fettle.Core.Internal
{
    internal interface ITestRunner
    {
        TestRunnerResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames);
    }
}