namespace Fettle.Core.Internal.MethodCoverage
{
    internal interface IMethodCoverage
    {
        bool IsMethodCovered(string method);
    }
}