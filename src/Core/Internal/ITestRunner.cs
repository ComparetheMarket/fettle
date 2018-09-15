using System;
using System.Collections.Generic;

namespace Fettle.Core.Internal
{
    internal interface ITestRunner
    {
        TestRunResult RunTests(
            IEnumerable<string> testAssemblyFilePaths,
            IEnumerable<string> testMethodNames);

        TestRunResult RunAllTests(
            IEnumerable<string> testAssemblyFilePaths);

        CoverageTestRunResult RunAllTestsAndAnalyseCoverage(
            IEnumerable<string> testAssemblyFilePaths,
            IDictionary<string, string> memberIdsToNames,
            Action<string, int> onAnalysingTestCase);
    }
}