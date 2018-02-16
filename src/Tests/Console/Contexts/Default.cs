using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fettle.Console;
using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Console.Contexts
{
    class Default
    {
        private readonly List<string> commandLineArgs = new List<string>();
        private IEventListener eventListener;

        protected Mock<IMutationTestRunner> MockMutationTestRunner { get; } = new Mock<IMutationTestRunner>();
        protected SpyOutputWriter SpyOutputWriter = new SpyOutputWriter();
        protected int ExitCode { get; private set; }

        protected void Given_config_file_does_not_exist()
        {
            commandLineArgs.Add("--config");
            commandLineArgs.Add($"a-non-existent-file-{Guid.NewGuid()}.yml");
        }
        
        protected void Given_mutation_testing_will_return_errors()
        {
            Given_a_valid_config_file();

            MockMutationTestRunner
                .Setup(r => r.Run(It.IsAny<Config>()))
                .Returns(Task.FromResult(new Result().WithError("an example error")));
        }

        protected void Given_a_valid_config_file()
        {
            var configFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Console", "fettle.config.yml");
            ModifyConfigFile(configFilePath);

            commandLineArgs.Add("--config");
            commandLineArgs.Add(configFilePath);
        }
        
        protected void Given_additional_command_line_arguments(params string[] args)
        {
            commandLineArgs.AddRange(args);
        }

        protected void Given_no_command_line_arguments_specified()
        {
            commandLineArgs.Clear();
        }

        protected void Given_some_mutants_will_survive()
        {
            var baseSlnDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..");

            var survivingMutant = new SurvivingMutant
            {
                SourceFilePath = Path.Combine(baseSlnDir, "someclass.cs"),
                SourceLine = 123,
                OriginalLine = "a+b",
                MutatedLine = "a-b"
            };

            MockMutationTestRunner
                .Setup(r => r.Run(It.IsAny<Config>()))
                .Callback(() =>
                {
                    // Raise events that a real MutationTestRunner would raise.
                    var classFilePath = Path.Combine(baseSlnDir, "someclass.cs");
                    eventListener.BeginMutationOfFile(classFilePath, baseSlnDir, 0, 1);
                    eventListener.SyntaxNodeMutating(0, 1);
                    eventListener.MutantSurvived(survivingMutant);
                    eventListener.EndMutationOfFile(classFilePath);
                })
                .Returns(Task.FromResult(new Result().WithSurvivingMutants(new []{ survivingMutant })));
        }

        protected void Given_no_mutants_will_survive()
        {
            MockMutationTestRunner
                .Setup(r => r.Run(It.IsAny<Config>()))
                .Returns(Task.FromResult(new Result()));
        }

        protected void Given_mutation_testing_will_throw_an_exception(Exception ex)
        {
            MockMutationTestRunner
                .Setup(r => r.Run(It.IsAny<Config>()))
                .Throws(ex);
        }

        protected void When_running_the_fettle_console_app()
        {
            ExitCode = Program.InternalEntryPoint(
                args: commandLineArgs.ToArray(),
                mutationTestRunnerFactory: CreateMutationTestRunner,
                outputWriter: SpyOutputWriter);
        }
        
        private IMutationTestRunner CreateMutationTestRunner(IEventListener eventListenerIn)
        {
            eventListener = eventListenerIn;
            return MockMutationTestRunner.Object;
        }

        private void ModifyConfigFile(string configFilePath)
        {
            var originalConfigFileContents = File.ReadAllText(configFilePath);
            File.WriteAllText(configFilePath, 
                string.Format(originalConfigFileContents, BuildConfig.AsString));
        }
    }
}