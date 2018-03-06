using System.Threading.Tasks;

namespace Fettle.Core
{
    public interface IMethodCoverage
    {
        Task<CoverageAnalysisResult> AnalyseMethodCoverage(Config config);
    }
}
