using System;
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

            var relativePath = RelativeDocumentPath(document, config);

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

            var relativePath = RelativeDocumentPath(document, config);

            return config.LocallyModifiedSourceFiles
                .Any(f => string.Equals(f, relativePath, StringComparison.InvariantCultureIgnoreCase));
        }

        private static string RelativeDocumentPath(Document document, Config config)
        {
            var baseDir = Path.GetFullPath(Path.GetDirectoryName(config.SolutionFilePath));
            var relativePath = document.FilePath.Substring(baseDir.Length + 1);
            return relativePath;
        }
    }
}