using System;
using System.Collections.Generic;

namespace Fettle.Core.Internal
{
    internal interface ICoverageTestRunner
    {
        CoverageTestRunResult RunAllTestsAndAnalyseCoverage(
            IEnumerable<string> testAssemblyFilePaths,
            IDictionary<string, string> memberIdsToNames,
            Action<string, int> onAnalysingTestCase);
    }
}
