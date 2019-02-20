using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        protected Mock<ITestRunner> MockTestRunner { get; } = new Mock<ITestRunner>();
        protected Mock<ITestRunnerFactory> MockTestRunnerFactory { get; } = new Mock<ITestRunnerFactory>();
        protected Mock<ICoverageAnalyser> MockCoverageAnalyser { get; } = new Mock<ICoverageAnalyser>();
        protected Mock<IMutationTestRunner> MockMutationTestRunner { get; } = new Mock<IMutationTestRunner>();
        protected Mock<ISourceControlIntegration> MockSourceControlIntegration { get; } = new Mock<ISourceControlIntegration>();
        protected SpyOutputWriter SpyOutputWriter = new SpyOutputWriter();

        protected int ExitCode { get; private set; }

        public Default()
        {
            MockTestRunnerFactory.Setup(x => x.CreateNUnitTestRunner()).Returns(MockTestRunner.Object);

            var emptyCoverageResult = new CoverageAnalysisResult();
            MockCoverageAnalyser
                .Setup(x => x.AnalyseCoverage(It.IsAny<Config>()))
                .Returns(
                    Task.FromResult<ICoverageAnalysisResult>(emptyCoverageResult));
        }

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
                .Returns(Task.FromResult(new MutationTestResult().WithError("an example error")));
        }

        protected void Given_coverage_analysis_will_return_an_error()
        {
            Given_a_valid_config_file();

            MockCoverageAnalyser
                .Setup(x => x.AnalyseCoverage(It.IsAny<Config>()))
                .Returns(
                    Task.FromResult(CoverageAnalysisResult.Error("an example coverage analysis error")));
        }

        protected void Given_a_valid_config_file_with_all_options_set() => Given_a_valid_config_file("fettle.config.alloptions.yml");

        protected void Given_a_valid_config_file() => Given_a_valid_config_file("fettle.config.yml");

        private void Given_a_valid_config_file(string filename)
        {
            var configFilePath =
                Path.Combine(TestContext.CurrentContext.TestDirectory, "Console", filename);

            var buildModeSpecificConfig = String.Format(File.ReadAllText(configFilePath), BuildConfig.AsString);
            File.WriteAllText(configFilePath, buildModeSpecificConfig);

            commandLineArgs.Add("--config");
            commandLineArgs.Add(configFilePath);
        }

        protected void Given_a_config_file_with_invalid_contents(Func<Config, Config> configModifier)
        {
            Given_a_config_file(configModifier);
        }

        protected void Given_a_config_file_with_custom_test_runner_specified(string testRunnerCommand)
        {
            Given_a_config_file(config =>
            {
                config.CustomTestRunnerCommand = testRunnerCommand;
                return config;
            });
        }

        protected void Given_a_config_file_where_all_source_files_are_filtered_out()
        {            
            Given_a_config_file(config =>
            {
                config.SourceFileFilters = new []{ @"SomeDir\SomeNonExistentFile.cs" };
                return config;
            });
        }

        private void Given_a_config_file(Func<Config, Config> configModifier)
        {
            var defaultConfig = new Config
            {
                SolutionFilePath = "../../../../../src/Examples/HasSurvivingMutants/HasSurvivingMutants.sln",
                TestAssemblyFilePaths = new [] { $"../../../../../src/Examples/HasSurvivingMutants/Tests/bin/{BuildConfig.AsString}/HasSurvivingMutants.Tests.dll" },
                ProjectFilters = new []{ "HasSurvivingMutants.Implementation" },
                SourceFileFilters = new []{ "Implementation/*.cs" }
            };

            var modifiedConfig = configModifier(defaultConfig);

            var configFileContents = $@"
solution: {modifiedConfig.SolutionFilePath}

testAssemblies: {CollectionToYamlList(modifiedConfig.TestAssemblyFilePaths)}

projectFilters: {CollectionToYamlList(modifiedConfig.ProjectFilters)}

sourceFileFilters: {CollectionToYamlList(modifiedConfig.SourceFileFilters)}

";

            if (modifiedConfig.CustomTestRunnerCommand != null)
            {
                configFileContents += $"customTestRunnerCommand: {modifiedConfig.CustomTestRunnerCommand}";
            }

            var baseDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Console");
            var configFilePath = Path.Combine(baseDir, "fettle.config.temp.yml");

            File.WriteAllText(configFilePath, configFileContents);

            commandLineArgs.Add("--config");
            commandLineArgs.Add(configFilePath);
        }

        private static string CollectionToYamlList(IEnumerable<string> collection)
        {
            var itemsAsYaml = collection.Select(item => $"{Environment.NewLine}    - {item ?? String.Empty}");
            return string.Join("", itemsAsYaml);
        }

        protected void Given_additional_command_line_arguments(params string[] args)
        {
            commandLineArgs.AddRange(args);
        }

        protected void Given_no_command_line_arguments_specified()
        {
            commandLineArgs.Clear();
        }
        
        protected void Given_coverage_analysis_is_disabled_via_command_line_argument()
        {
            commandLineArgs.Add("--skipcoverageanalysis");
        }

        protected void Given_some_mutants_will_survive()
        {
            var baseSlnDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..");

            var survivingMutant = new Mutant
            {
                SourceFilePath = Path.Combine(baseSlnDir, "someclass.cs"),
                SourceLine = 123,
                OriginalLine = "a+b",
                MutatedLine = "a-b"
            };
            var killedMutant = new Mutant
            {
                SourceFilePath = Path.Combine(baseSlnDir, "someotherclass.cs"),
                SourceLine = 321,
                OriginalLine = "a > 0",
                MutatedLine = "a >= 0"
            };
            var skippedMutant = new Mutant
            {
                SourceFilePath = Path.Combine(baseSlnDir, "yetanotherclass.cs"),
                SourceLine = 456,
                OriginalLine = "a == 0",
                MutatedLine = "a != 0"
            };

            MockMutationTestRunner
                .Setup(r => r.Run(It.IsAny<Config>()))
                .Callback(() =>
                {
                    // Raise events that a real MutationTestRunner would raise.
                    var classFilePath = Path.Combine(baseSlnDir, "someclass.cs");
                    eventListener.BeginMutationOfFile(classFilePath, baseSlnDir, 0, 1);
                    eventListener.MemberMutating("System.Void SomeProject.SomeOtherNamespace.SomeClass::SomeMethod(System.Int32)");
                    eventListener.SyntaxNodeMutating(0, 1);
                    eventListener.MutantSurvived(survivingMutant);
                    eventListener.MutantKilled(killedMutant);
                    eventListener.MutantSkipped(skippedMutant, "skip reason");
                    eventListener.EndMutationOfFile(classFilePath);
                })
                .Returns(Task.FromResult(new MutationTestResult().WithSurvivingMutants(new []{ survivingMutant })));
        }

        protected void Given_coverage_analysis_runs_successfully()
        {
            ICoverageAnalysisResult emptyCoverageResult = new CoverageAnalysisResult();
            MockCoverageAnalyser
                .Setup(x => x.AnalyseCoverage(It.IsAny<Config>()))
                .Callback(() =>
                {
                    for (var i = 0; i < 10; ++i)
                    {
                        eventListener.BeginCoverageAnalysisOfTestCase($"Test{i}", i, 10);
                    }
                })
                .Returns(
                    Task.FromResult(emptyCoverageResult));
        }

        protected void Given_no_mutants_will_survive()
        {
            MockMutationTestRunner
                .Setup(r => r.Run(It.IsAny<Config>()))
                .Returns(Task.FromResult(new MutationTestResult()));
        }

        protected void Given_mutation_testing_will_throw_an_exception(Exception ex)
        {
            MockMutationTestRunner
                .Setup(r => r.Run(It.IsAny<Config>()))
                .Throws(ex);
        }

        protected void Given_coverage_analysis_will_throw_an_exception(Exception ex)
        {
            MockCoverageAnalyser
                .Setup(r => r.AnalyseCoverage(It.IsAny<Config>()))
                .Throws(ex);
        }

        protected void When_running_the_fettle_console_app()
        {
            ICoverageAnalyser CreateMockCoverageAnalyser(IEventListener eventListenerIn)
            {
                eventListener = eventListenerIn;
                return MockCoverageAnalyser.Object;
            }

            IMutationTestRunner CreateMockMutationTestRunner(
                ITestRunner _,
                IEventListener eventListenerIn, 
                ICoverageAnalysisResult __)
            {
                return MockMutationTestRunner.Object;
            }

            ExitCode = Program.Run(
                args: commandLineArgs.ToArray(),
                testRunnerFactory: MockTestRunnerFactory.Object,
                mutationTestRunnerFactory: CreateMockMutationTestRunner,
                coverageAnalyserFactory: CreateMockCoverageAnalyser,
                sourceControlIntegration: MockSourceControlIntegration.Object,
                outputWriter: SpyOutputWriter);
        }
    }
}