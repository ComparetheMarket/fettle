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
                SolutionFilePath = NormalisePathSeparators(
                    Path.Combine(baseDirectory, SolutionFilePath)),

                TestAssemblyFilePaths = TestAssemblyFilePaths.Select(x => 
                        NormalisePathSeparators(    
                            Path.Combine(baseDirectory, x)))
                    .ToArray(),

                NunitTestRunnerFilePath = NormalisePathSeparators(
                    Path.Combine(baseDirectory, NunitTestRunnerFilePath)),
                
                ProjectFilters = ProjectFilters?.ToArray(),
                SourceFileFilters = SourceFileFilters?.ToArray(),

                CoverageReportFilePath = CoverageReportFilePath != null
                    ? NormalisePathSeparators(Path.Combine(baseDirectory, CoverageReportFilePath))
                    : null
            };
        }

        private string NormalisePathSeparators(string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }
    }
}