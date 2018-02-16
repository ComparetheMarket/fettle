namespace Fettle.Core.Internal.MethodCoverage
{
    internal interface IMethodCoverage
    {
        bool AnyMethodsCovered { get; }
        bool IsMethodCovered(string method);
    }
}