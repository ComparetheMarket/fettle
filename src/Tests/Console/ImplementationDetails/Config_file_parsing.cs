using Fettle.Console;
using NUnit.Framework;

namespace Fettle.Tests.Console.ImplementationDetails
{
    class Config_file_parsing
    {
        [Test]
        public void All_options_can_be_parsed()
        {
            const string fileContents = @"

solution: src/Examples/HasSurvivingMutants/HasSurvivingMutants.sln

testAssemblies:
    - src/Examples/HasSurvivingMutants/Tests/bin/{0}/HasSurvivingMutants.Tests.dll

projectFilters:
    - HasSurvivingMutants.Implementation

sourceFileFilters:
    - Implementation/*.cs

customTestRunnerCommand: src/Examples/HasSurvivingMutants/run-tests.bat arg=123

";
            var parsedConfig = ConfigFile.Parse(fileContents);

            Assert.That(parsedConfig.SolutionFilePath, Is.EqualTo("src/Examples/HasSurvivingMutants/HasSurvivingMutants.sln"));
            Assert.That(parsedConfig.TestAssemblyFilePaths, Is.EquivalentTo(new[]{ "src/Examples/HasSurvivingMutants/Tests/bin/{0}/HasSurvivingMutants.Tests.dll" }));
            Assert.That(parsedConfig.ProjectFilters, Is.EquivalentTo(new[]{ "HasSurvivingMutants.Implementation" }));
            Assert.That(parsedConfig.SourceFileFilters, Is.EquivalentTo(new[]{ "Implementation/*.cs" }));
            Assert.That(parsedConfig.CustomTestRunnerCommand, Is.EqualTo("src/Examples/HasSurvivingMutants/run-tests.bat arg=123"));
        }
    }
}
