using System.Collections.Generic;
using System.Linq;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitTestRunner : NUnitTestEngine, ITestRunner
    {
        public TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            return RunTests(testAssemblyFilePaths, testMethodNames, new NullEventListener());
        }

        public TestRunResult RunAllTests(IEnumerable<string> testAssemblyFilePaths)
        {
            var testAssemblyFilePathsAsArray = testAssemblyFilePaths.ToArray();
            return RunTests(testAssemblyFilePathsAsArray, FindTestsInAssemblies(testAssemblyFilePathsAsArray));
        }
    }
}