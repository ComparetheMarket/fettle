using System.Collections.Generic;

namespace Fettle.Core.Internal.MethodCoverage
{
    internal class MethodCoverage : IMethodCoverage
    {
        private readonly ISet<string> coveredMethods;

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