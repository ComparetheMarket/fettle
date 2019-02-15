using System.Collections.Generic;

namespace Fettle.Core
{
    public interface ITestRunner
    {
        TestRunResult RunTests(
            IEnumerable<string> testAssemblyFilePaths,
            IEnumerable<string> testMethodNames);

        TestRunResult RunAllTests(
            IEnumerable<string> testAssemblyFilePaths);
    }
}