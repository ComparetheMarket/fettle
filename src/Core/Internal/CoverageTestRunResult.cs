using System.Collections.Generic;
using System.Collections.Immutable;

namespace Fettle.Core.Internal
{
    internal class CoverageTestRunResult : TestRunResult
    {
        public IDictionary<string, ImmutableHashSet<string>> MethodsAndCoveringTests { get; set; }
    }
}