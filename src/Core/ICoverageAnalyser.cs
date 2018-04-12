using System.Threading.Tasks;

namespace Fettle.Core
{
    public interface ICoverageAnalyser
    {
        Task<ICoverageAnalysisResult> AnalyseMethodCoverage(Config config);
    }
}
