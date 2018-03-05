using System.Threading.Tasks;

namespace Fettle.Core
{
    public interface IMethodCoverage
    {
        Task Initialise(Config config);
        string[] TestsThatCoverMethod(string fullMethodName);
    }
}
