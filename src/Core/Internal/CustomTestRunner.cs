using System.Collections.Generic;

namespace Fettle.Core.Internal
{
    internal class CustomTestRunner : ITestRunner
    {
        public TestRunResult RunAllTests(IEnumerable<string> testAssemblyFilePaths)
        {
            throw new System.NotImplementedException();
        }

        public TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            throw new System.NotImplementedException();
        }
    }
}
