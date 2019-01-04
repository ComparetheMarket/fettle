using System.Linq;
using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Console
{
    class Configuration : Contexts.Default
    {
        public Configuration()
        {
            Given_a_valid_config_file();

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_the_options_specified_in_the_config_file_are_used()
        {
            MockMutationTestRunner.Verify(x => x.Run(
                It.Is<Config>(c => 
                    c.SolutionFilePath.EndsWith("../../../../../src/Examples/HasSurvivingMutants/HasSurvivingMutants.sln")
                    && c.TestAssemblyFilePaths.Single().EndsWith($"../../../../../src/Examples/HasSurvivingMutants/Tests/bin/{BuildConfig.AsString}/HasSurvivingMutants.Tests.dll")
                    && c.ProjectFilters.Single() == "HasSurvivingMutants.Implementation"
                    && c.SourceFileFilters.Single() == "Implementation/*.cs"
                    && c.CustomTestRunnerCommand == "src/Examples/HasSurvivingMutants/run-tests.bat arg=123"
                )));
        }
    }
}