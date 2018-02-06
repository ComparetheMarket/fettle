using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Assembly_modification : Contexts.Default
    {
        private readonly Dictionary<string, DateTime> previousModificationTimes;

        public Assembly_modification()
        {
            Given_a_partially_tested_app_in_which_a_mutant_will_survive();

            previousModificationTimes = ModificationTimesOfSourceAssemblies();

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_the_original_assemblies_are_not_modified() // E.g. because we copy the files to a temporary directory.
        {
            var newModificationTimes = ModificationTimesOfSourceAssemblies();
            foreach (var prevEntry in previousModificationTimes)
            {
                var newValue = newModificationTimes[prevEntry.Key];
                Assert.That(prevEntry.Value, Is.EqualTo(newValue),
                    $"\"{prevEntry.Key}\" appears to have been modified");
            }
        }

        private Dictionary<string, DateTime> ModificationTimesOfSourceAssemblies()
        {
            var result = new Dictionary<string, DateTime>();
            foreach (var directory in Config.TestAssemblyFilePaths.Select(Path.GetDirectoryName))
            {
                foreach (var filePath in Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories))
                {
                    result.Add(filePath, File.GetLastWriteTime(filePath));
                }
            }
            return result;
        }
    }
}