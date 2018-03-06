using System.Threading.Tasks;

namespace Fettle.Core
{
    public interface IMutationTestRunner
    {
        Task<MutationTestResult> Run(Config config);
    }
}