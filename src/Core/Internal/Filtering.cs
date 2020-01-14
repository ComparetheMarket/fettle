using System;
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

            var relativePath = RelativeFilePath(project.FilePath, config);

            return config.ProjectFilters
                .Any(f => Glob.Parse(f).IsMatch(project.Name) || Glob.Parse(f).IsMatch(relativePath));
        }

        public static bool ShouldMutateDocument(Document document, Config config)
        {
            return ShouldMutateAccordingToFilters(document, config) &&
                   ShouldMutateAccordingToLocallyModifiedList(document, config);
        }

        private static bool ShouldMutateAccordingToFilters(Document document, Config config)
        {
            if (config.SourceFileFilters == null ||
                !config.SourceFileFilters.Any())
            {
                return true;
            }

            var relativePath = RelativeFilePath(document.FilePath, config);

            var matchesAnyFilter = config.SourceFileFilters
                .Any(f => Glob.Parse(f).IsMatch(relativePath));

            return matchesAnyFilter;
        }

        private static bool ShouldMutateAccordingToLocallyModifiedList(Document document, Config config)
        {
            if (config.LocallyModifiedSourceFiles == null)
            {
                return true;
            }

            var relativePath = RelativeFilePath(document.FilePath, config);

            return config.LocallyModifiedSourceFiles
                .Any(f => string.Equals(f, relativePath, StringComparison.InvariantCultureIgnoreCase));
        }

        private static string RelativeFilePath(string filePath, Config config)
        {
	        var baseDir = config.GetSolutionFolder();
            var relativePath = filePath.Substring(baseDir.Length + 1);
            return relativePath;
        }
    }
}