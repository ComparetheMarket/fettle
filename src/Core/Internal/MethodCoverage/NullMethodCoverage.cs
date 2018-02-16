namespace Fettle.Core.Internal.MethodCoverage
{
    internal class NullMethodCoverage : IMethodCoverage
    {
        public bool AnyMethodsCovered => true;

        public bool IsMethodCovered(string method)
        {
            return true;
        }
    }
}