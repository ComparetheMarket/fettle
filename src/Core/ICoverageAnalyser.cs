using System.Threading.Tasks;

namespace Fettle.Core
{
    public interface ICoverageAnalyser
    {
        Task<CoverageAnalysisResult> AnalyseMethodCoverage(Config config);
    }
}
