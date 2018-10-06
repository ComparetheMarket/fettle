using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis;

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
 
        // Auto-generated
        //
        public string[] LocallyModifiedSourceFiles { get; set; }

        public Config WithPathsRelativeTo(string baseDirectory)
        {
            return new Config
            {
                SolutionFilePath = SolutionFilePath != null
                    ? Path.Combine(baseDirectory, SolutionFilePath)
                    : null,

                TestAssemblyFilePaths = TestAssemblyFilePaths?
                    .Select(x => x != null ? Path.Combine(baseDirectory, x) : null)
                    .ToArray(),

                ProjectFilters = ProjectFilters?.ToArray(),
                SourceFileFilters = SourceFileFilters?.ToArray()
            };
        }

        public IEnumerable<string> Validate()
        {
            return ValidateRequiredPropertiesArePresent()
                   .Concat(ValidateContentsOfCollections())
                   .Concat(ValidateFilesArePresent());
        }

        public async Task<Document[]> FindMutatableDocuments()
        {
            using (var workspace = MSBuildWorkspaceFactory.Create())
            {
                var solution = await workspace.OpenSolutionAsync(SolutionFilePath);
                return solution.MutatableClasses(this);
            }
        }

        public async Task<bool> HasAnyMutatableDocuments() => (await FindMutatableDocuments()).Any();

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

            if (TestAssemblyFilePaths != null)
            {
                var nonExistentTestAssemblies = TestAssemblyFilePaths.Where(f => !File.Exists(f)).ToList();
                if (nonExistentTestAssemblies.Any())
                {
                    var filesListMessage = string.Join(Environment.NewLine, nonExistentTestAssemblies);
                    yield return
                        $"One or more test assemblies were not found:{Environment.NewLine}{filesListMessage}";
                }
            }
        }

        private IEnumerable<string> ValidateContentsOfCollections()
        {
            bool AnyItemsNull(IEnumerable<string> items) => items != null && items.Any(i => i == null);

            if (AnyItemsNull(TestAssemblyFilePaths))
            {
                yield return "One or more items in the list of test assemblies is blank";
            }

            if (AnyItemsNull(ProjectFilters))
            {
                yield return "One or more items in the list of project filters is blank";
            }
            
            if (AnyItemsNull(SourceFileFilters))
            {
                yield return "One or more items in the list of source file filters is blank";
            }
        }
    }
}