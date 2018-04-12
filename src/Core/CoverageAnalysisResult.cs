using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Fettle.Core
{
    public class CoverageAnalysisResult : ICoverageAnalysisResult
    {
        private Dictionary<string, IDictionary<string, ImmutableHashSet<string>>> coverageByTestAssembly = 
            new Dictionary<string, IDictionary<string, ImmutableHashSet<string>>>();

        public string ErrorDescription { get; private set; }

        public string[] AllAnalysedMethods =>
            coverageByTestAssembly
                .Select(x => x.Value)
                .SelectMany(x => x.Keys)
                .ToArray();
        
        public string[] TestsThatCoverMethod(string method, string testAssemblyFilePath)
        {
            var coverageForTestAssembly = coverageByTestAssembly[testAssemblyFilePath];
            return coverageForTestAssembly.TryGetValue(method, out var tests) ? tests.ToArray() : new string[0];
        }

        public bool IsMethodCovered(string method)
        {
            var testAssemblies = coverageByTestAssembly.Keys;
            return testAssemblies.Any(x => TestsThatCoverMethod(method, x).Any());
        }

        internal static ICoverageAnalysisResult Error(string errorDescription)
        {
            return new CoverageAnalysisResult
            {
                ErrorDescription = errorDescription
            };
        }

        internal CoverageAnalysisResult WithCoveredMethods(
            IDictionary<string,ImmutableHashSet<string>> methodsAndTheirCoveringTests,
            string testAssemblyFilePath)
        {
            var result = new CoverageAnalysisResult{ coverageByTestAssembly = coverageByTestAssembly };
            result.coverageByTestAssembly.Add(testAssemblyFilePath, methodsAndTheirCoveringTests);
            return result;
        }
    }
}