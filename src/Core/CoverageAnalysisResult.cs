using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Fettle.Core
{
    public class CoverageAnalysisResult : ICoverageAnalysisResult
    {
        private Dictionary<string, IDictionary<string, ImmutableHashSet<string>>> coverageByTestAssembly = 
            new Dictionary<string, IDictionary<string, ImmutableHashSet<string>>>();

        public bool WasSuccessful => ErrorDescription == null;

        public string ErrorDescription { get; private set; }

        public string[] AllAnalysedMembers =>
            coverageByTestAssembly
                .Select(x => x.Value)
                .SelectMany(x => x.Keys)
                .ToArray();
        
        public string[] TestsThatCoverMember(string member, string testAssemblyFilePath)
        {
            var coverageForTestAssembly = coverageByTestAssembly[testAssemblyFilePath];
            return coverageForTestAssembly.TryGetValue(member, out var tests) ? tests.ToArray() : new string[0];
        }

        public bool IsMemberCovered(string member)
        {
            var testAssemblies = coverageByTestAssembly.Keys;
            return testAssemblies.Any(x => TestsThatCoverMember(member, x).Any());
        }

        internal static ICoverageAnalysisResult Error(string errorDescription)
        {
            return new CoverageAnalysisResult
            {
                ErrorDescription = errorDescription
            };
        }

        internal CoverageAnalysisResult WithCoveredMembers(
            IDictionary<string,ImmutableHashSet<string>> membersAndTheirCoveringTests,
            string testAssemblyFilePath)
        {
            var result = new CoverageAnalysisResult{ coverageByTestAssembly = coverageByTestAssembly };
            result.coverageByTestAssembly.Add(testAssemblyFilePath, membersAndTheirCoveringTests);
            return result;
        }
    }
}