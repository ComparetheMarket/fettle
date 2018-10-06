using System.IO;
using System.Linq;
using DotNet.Globbing;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{
    internal static class Filtering
    {
        public static bool ShouldMutateProject(Project project, Config config)
        {
            if (config.ProjectFilters == null ||
                !config.ProjectFilters.Any())
            {
                return true;
            }

            return config.ProjectFilters
                .Any(f => Glob.Parse(f).IsMatch(project.Name));
        }

        public static bool ShouldMutateDocument(Document @class, Config config)
        {
            if (config.SourceFileFilters == null ||
                !config.SourceFileFilters.Any())
            {
                return true;
            }

            var baseDir = Path.GetFullPath(Path.GetDirectoryName(config.SolutionFilePath));
            var relativePath = @class.FilePath.Substring(baseDir.Length + 1);
            return config.SourceFileFilters
                .Any(f => Glob.Parse(f).IsMatch(relativePath));
        }
    }
}