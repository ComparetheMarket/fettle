using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Fettle.Core
{
    public class CoverageAnalysisResult
    {
        public IReadOnlyDictionary<string,ImmutableHashSet<string>> MethodsAndTheirCoveringTests { get; private set; } =
            new ConcurrentDictionary<string, ImmutableHashSet<string>>();
            
        public string ErrorDescription { get; private set; }

        internal static CoverageAnalysisResult Error(string errorDescription)
        {
            return new CoverageAnalysisResult
            {
                ErrorDescription = errorDescription
            };
        }

        internal static CoverageAnalysisResult Success(IDictionary<string,ImmutableHashSet<string>> methodsAndTheirCoveringTests)
        {
            return new CoverageAnalysisResult
            {
                MethodsAndTheirCoveringTests = new Dictionary<string, ImmutableHashSet<string>>(methodsAndTheirCoveringTests)
            };
        }
    }
}