using System.IO;
using Fettle.Console;
using NUnit.Framework;

namespace Fettle.Tests.Console.ImplementationDetails
{
    class Config_file_parsing
    {
        [Test]
        public void All_properties_are_parsed()
        {
            const string fileContents = @"

solution: src/blah.sln

testAssemblies:
    - src/Blah.Tests/bin/Debug/Blah.Tests.dll
    - src/Blurg.Tests/bin/Debug/Blurg.Tests.dll

nunitTestRunner: nunit/nunit3-console.exe

projectFilters:
    - BlahImpl

sourceFileFilters: [ Blah/Things/*.cs, Wibble/w*mble.cs ]

coverageReport: report.xml
";
            var baseDir = Path.Combine(Path.GetPathRoot(Path.GetTempPath()), "random");

            var config = ConfigFile.Parse(fileContents).WithPathsRelativeTo(baseDir);

            Assert.That(config.NunitTestRunnerFilePath, Is.EqualTo(Path.Combine(baseDir, "nunit", "nunit3-console.exe")));
            Assert.That(config.SolutionFilePath, Is.EqualTo(Path.Combine(baseDir, "src" ,"blah.sln")));
            Assert.That(config.TestAssemblyFilePaths, Is.EquivalentTo(new[]
            {
                Path.Combine(baseDir, "src", "Blah.Tests", "bin", "Debug", "Blah.Tests.dll"),
                Path.Combine(baseDir, "src", "Blurg.Tests", "bin", "Debug", "Blurg.Tests.dll")
            }));
            Assert.That(config.ProjectFilters, Is.EquivalentTo(new[] { @"BlahImpl" }));
            Assert.That(config.SourceFileFilters, Is.EquivalentTo(new[] { @"Blah/Things/*.cs", @"Wibble/w*mble.cs" }));
            Assert.That(config.CoverageReportFilePath, Is.EqualTo(Path.Combine(baseDir, "report.xml")));
        }

        [Test]
        public void Optional_properties_can_be_omitted()
        {
            const string fileContents = @"

solution: src/blah.sln

testAssemblies:
    - src/Blah.Tests/bin/Debug/Blah.Tests.dll
    - src/Blurg.Tests/bin/Debug/Blurg.Tests.dll

nunitTestRunner: nunit/nunit3-console.exe

#projectFilters: [ BlahImpl ]
#sourceFileFilters: [ Blah/Things/*.cs, Wibble/w*mble.cs ]
#coverageReport: report.xml
";
            var baseDir = Path.Combine(Path.GetPathRoot(Path.GetTempPath()), "random");

            var config = ConfigFile.Parse(fileContents).WithPathsRelativeTo(baseDir);

            Assert.That(config.NunitTestRunnerFilePath, Is.EqualTo(Path.Combine(baseDir, "nunit", "nunit3-console.exe")));
            Assert.That(config.SolutionFilePath, Is.EqualTo(Path.Combine(baseDir, "src", "blah.sln")));
            Assert.That(config.TestAssemblyFilePaths, Is.EquivalentTo(new[]
            {
                Path.Combine(baseDir, "src", "Blah.Tests", "bin", "Debug", "Blah.Tests.dll"),
                Path.Combine(baseDir, "src", "Blurg.Tests", "bin", "Debug", "Blurg.Tests.dll")
            }));

            Assert.That(config.ProjectFilters, Is.Null);
            Assert.That(config.SourceFileFilters, Is.Null);
            Assert.That(config.CoverageReportFilePath, Is.Null);
        }
    }
}
