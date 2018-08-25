using System.Threading.Tasks;

namespace Fettle.Core
{
    public interface ICoverageAnalyser
    {
        Task<ICoverageAnalysisResult> AnalyseCoverage(Config config);
    }
}
