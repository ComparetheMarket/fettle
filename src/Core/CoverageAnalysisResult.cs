using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MethodsAndTheirCoveringTests =
    System.Collections.Generic.IDictionary<string, System.Collections.Immutable.ImmutableHashSet<string>>;

namespace Fettle.Core
{
    public class CoverageAnalysisResult
    {
        private Dictionary<string, MethodsAndTheirCoveringTests> coverageByTestAssembly = 
            new Dictionary<string, MethodsAndTheirCoveringTests>();

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

        internal static CoverageAnalysisResult Error(string errorDescription)
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