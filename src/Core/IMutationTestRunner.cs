using System.Threading.Tasks;

namespace Fettle.Core
{
    public interface IMutationTestRunner
    {
        Task<Result> Run(Config config);
    }
}