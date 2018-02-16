using System.Collections.Generic;
using System.Linq;

namespace Fettle.Core.Internal.MethodCoverage
{
    internal class MethodCoverage : IMethodCoverage
    {
        private readonly ISet<string> coveredMethods;

        public bool AnyMethodsCovered => coveredMethods.Any();

        public MethodCoverage(ISet<string> coveredMethods)
        {
            this.coveredMethods = coveredMethods;
        }

        public bool IsMethodCovered(string method)
        {
            return coveredMethods.Contains(method);
        }
    }
}