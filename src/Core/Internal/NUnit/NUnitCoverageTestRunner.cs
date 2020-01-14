using System;
using System.Collections.Generic;
using System.Linq;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitCoverageTestRunner : NUnitTestEngine, ICoverageTestRunner
    {
        public CoverageTestRunResult RunAllTestsAndAnalyseCoverage(
            IEnumerable<string> testAssemblyFilePaths,
            IDictionary<string, string> memberIdsToNames,
            Action<string, int> onAnalysingTestCase,
            Action<string> onMemberExecuted)
        {
            var coverageCollector = new NUnitCoverageCollector(memberIdsToNames, onAnalysingTestCase, onMemberExecuted);

            var testAssemblyFilePathsAsArray = testAssemblyFilePaths.ToArray();
            var availableTests = FindTestsInAssemblies(testAssemblyFilePathsAsArray);
            var runTestsResult = RunTests(testAssemblyFilePathsAsArray, availableTests, coverageCollector);

            return new CoverageTestRunResult
            {
                Status = runTestsResult.Status,
                Error = runTestsResult.Error,
                MembersAndCoveringTests = coverageCollector.MembersAndCoveringTests.ToDictionary(x => x.Key, x => x.Value)
            };
        }
    }
}