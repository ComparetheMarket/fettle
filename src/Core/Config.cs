using System.IO;
using System.Linq;

namespace Fettle.Core
{
    public class Config
    {
        public string SolutionFilePath { get; set; }        
        public string[] TestAssemblyFilePaths { get; set; }

        public string NunitTestRunnerFilePath { get; set; }
        
        public string[] ProjectFilters { get; set; }
        public string[] SourceFileFilters { get; set; }
        
        public string CoverageReportFilePath { get; set; }
        
        public Config WithPathsRelativeTo(string baseDirectory)
        {
            return new Config
            {
                SolutionFilePath = Path.Combine(baseDirectory, SolutionFilePath),
                TestAssemblyFilePaths = TestAssemblyFilePaths.Select(x => Path.Combine(baseDirectory, x)).ToArray(),
                NunitTestRunnerFilePath = Path.Combine(baseDirectory, NunitTestRunnerFilePath),
                
                ProjectFilters = ProjectFilters?.ToArray(),
                SourceFileFilters = SourceFileFilters?.ToArray(),
                CoverageReportFilePath = CoverageReportFilePath != null ? Path.Combine(baseDirectory, CoverageReportFilePath) : null
            };
        }
    }
}