using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fettle.Core
{
    public class Config
    {
        // Required
        //
        public string SolutionFilePath { get; set; }
        public string[] TestAssemblyFilePaths { get; set; }
        public string[] ProjectFilters { get; set; }
 
        // Optional
        //
        public string[] SourceFileFilters { get; set; }
 
        public Config WithPathsRelativeTo(string baseDirectory)
        {
            var result = new Config();

            result.SolutionFilePath = SolutionFilePath != null
                ? Path.Combine(baseDirectory, SolutionFilePath)
                : null;

            if (TestAssemblyFilePaths != null)
            {
                result.TestAssemblyFilePaths = TestAssemblyFilePaths
                    .Select(x => x != null ? Path.Combine(baseDirectory, x) : null)
                    .ToArray();
            }

            result.ProjectFilters = ProjectFilters?.ToArray();
            result.SourceFileFilters = SourceFileFilters?.ToArray();
 
            return result;
        }

        public IEnumerable<string> Validate()
        {
            return ValidateRequiredPropertiesArePresent()
                   .Concat(ValidateFilesArePresent());
        }
        
        private IEnumerable<string> ValidateRequiredPropertiesArePresent()
        {
            string PropertyNotSpecified(string propertyName)
            {
                return $"The property \"{propertyName}\" is required but hasn't been specified in the configuration.";
            }

            if (SolutionFilePath == null)
            {
                yield return PropertyNotSpecified(nameof(SolutionFilePath));
            }
            
            if (TestAssemblyFilePaths == null || !TestAssemblyFilePaths.Any())
            {
                yield return PropertyNotSpecified(nameof(TestAssemblyFilePaths));
            }
        }

        private IEnumerable<string> ValidateFilesArePresent()
        {
            if (SolutionFilePath != null && !File.Exists(SolutionFilePath))
            {
                yield return
                    $"The solution file was not found: \"{SolutionFilePath}\"";
            }

            var nonExistentTestAssemblies = TestAssemblyFilePaths.Where(f => !File.Exists(f)).ToList();
            if (nonExistentTestAssemblies.Any())
            {
                var filesListMessage = string.Join(Environment.NewLine, nonExistentTestAssemblies);
                yield return
                    $"One or more test assemblies were not found:{Environment.NewLine}{filesListMessage}";
            }
        }
    }
}