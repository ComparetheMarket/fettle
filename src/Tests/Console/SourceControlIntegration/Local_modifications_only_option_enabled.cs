using System.Linq;
using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Console.SourceControlIntegration
{
    class Local_modifications_only_option_enabled : Contexts.SourceControlIntegration
    {
        public Local_modifications_only_option_enabled()
        {
            Given_a_valid_config_file();
            Given_additional_command_line_arguments("--modificationsonly");
            Given_locally_modified_files(@"Implementation\PartiallyTestedNumberComparison.cs");

            When_running_the_fettle_console_app();
        }

        [Test]
        public void Then_only_locally_modified_source_files_are_considered_for_mutation()
        {
            MockMutationTestRunner.Verify(r => 
                r.Run(It.Is<Config>(c =>
                    c.LocallyModifiedSourceFiles.Length == 1 && 
                    c.LocallyModifiedSourceFiles.Single() == @"Implementation\PartiallyTestedNumberComparison.cs"
                )));
        }
    }
}
