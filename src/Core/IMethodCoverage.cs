namespace Fettle.Core
{
    public interface IMethodCoverage
    {
        string[] TestsThatCoverMethod(string fullMethodName);
    }
}
