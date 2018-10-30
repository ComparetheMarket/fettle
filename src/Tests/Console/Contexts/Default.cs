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
        
        protected Mock<ICoverageAnalyser> MockCoverageAnalyser { get; } = new Mock<ICoverageAnalyser>();
        protected Mock<IMutationTestRunner> MockMutationTestRunner { get; } = new Mock<IMutationTestRunner>();
        protected Mock<ISourceControlIntegration> MockSourceControlIntegration { get; } = new Mock<ISourceControlIntegration>();
        protected SpyOutputWriter SpyOutputWriter = new SpyOutputWriter();

        protected int ExitCode { get; private set; }

        public Default()
        {
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

        protected void Given_a_valid_config_file()
        {
            var configFilePath =
                Path.Combine(TestContext.CurrentContext.TestDirectory, "Console", "fettle.config.yml");

            var buildModeSpecificConfig = String.Format(File.ReadAllText(configFilePath), BuildConfig.AsString);
            File.WriteAllText(configFilePath, buildModeSpecificConfig);

            commandLineArgs.Add("--config");
            commandLineArgs.Add(configFilePath);
        }

        protected void Given_a_config_file_with_invalid_contents(Func<Config, Config> configModifier)
        {
            Given_a_config_file(configModifier);
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
                ProjectFilters = new []{ "HasSurvivingMutants.Implementation" },
                TestProjectFilters = new[] { "HasSurvivingMutants.Tests" },
                SourceFileFilters = new []{ "Implementation/*.cs" }
            };

            var modifiedConfig = configModifier(defaultConfig);

            var configFileContents = $@"
solution: {modifiedConfig.SolutionFilePath}

projectFilters: {CollectionToYamlList(modifiedConfig.ProjectFilters)}

testProjectFilters: {CollectionToYamlList(modifiedConfig.TestProjectFilters)}

sourceFileFilters: {CollectionToYamlList(modifiedConfig.SourceFileFilters)}
";
            var configFilePath =
                Path.Combine(TestContext.CurrentContext.TestDirectory, "Console", "fettle.config.invalid.yml");

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
                    eventListener.MemberMutating("System.Void SomeProject.SomeOtherNamespace.SomeClass::SomeMethod(System.Int32)");
                    eventListener.SyntaxNodeMutating(0, 1);
                    eventListener.MutantSurvived(survivingMutant);
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
                IEventListener eventListenerIn, 
                ICoverageAnalysisResult _)
            {
                return MockMutationTestRunner.Object;
            }

            ExitCode = Program.InternalEntryPoint(
                args: commandLineArgs.ToArray(),
                mutationTestRunnerFactory: CreateMockMutationTestRunner,
                coverageAnalyserFactory: CreateMockCoverageAnalyser,
                sourceControlIntegration: MockSourceControlIntegration.Object,
                outputWriter: SpyOutputWriter);
        }        
    }
}