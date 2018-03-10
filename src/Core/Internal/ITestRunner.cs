using System.Collections.Generic;
using System.Collections.Immutable;

namespace Fettle.Core.Internal
{
    internal interface ITestRunner
    {
        TestRunResult RunTests(
            IEnumerable<string> testAssemblyFilePaths,
            IEnumerable<string> testMethodNames);

        TestRunResult RunTestsAndCollectExecutedMethods(
            IEnumerable<string> testAssemblyFilePaths,
            IEnumerable<string> testMethodNames,
            IDictionary<string, string> methodIdsToNames,
            IDictionary<string, ImmutableHashSet<string>> methodsAndCoveringTests);
    }
}