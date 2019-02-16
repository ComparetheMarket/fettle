using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Fettle.Core;

namespace Fettle.Console
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            IMutationTestRunner CreateRealMutationTestRunner(
                ITestRunner testRunner,
                IEventListener eventListener,
                ICoverageAnalysisResult coverageAnalysisResult)
            {
                return new MutationTestRunner(
                    testRunner,
                    coverageAnalysisResult, 
                    eventListener);
            }

            ICoverageAnalyser CreateRealCoverageAnalyser(IEventListener eventListener) => new CoverageAnalyser(eventListener);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<TestRunnerFactory>().As<ITestRunnerFactory>();
            containerBuilder.RegisterType<GitIntegration>().As<ISourceControlIntegration>();
            containerBuilder.RegisterType<ConsoleOutputWriter>().As<IOutputWriter>();
            containerBuilder.RegisterInstance<Func<IEventListener, ICoverageAnalyser>>(CreateRealCoverageAnalyser);
            containerBuilder.RegisterInstance<Func<ITestRunner, IEventListener, ICoverageAnalysisResult, IMutationTestRunner>>(CreateRealMutationTestRunner);
            var container = containerBuilder.Build();

            return Run(args, container);
        }

        private static class ExitCodes
        {
            public const int Success = 0;
            public const int SomeMutantsSurvived = 1;
            public const int ConfigOrArgsAreInvalid = 2;
            public const int UnexpectedError = 3;
        }

        internal static int Run(string[] args, IContainer diContainer)
        {
            return Run(
                args,
                diContainer.Resolve<ITestRunnerFactory>(),
                diContainer.Resolve<Func<ITestRunner, IEventListener, ICoverageAnalysisResult, IMutationTestRunner>>(),
                diContainer.Resolve<Func<IEventListener, ICoverageAnalyser>>(),
                diContainer.Resolve<ISourceControlIntegration>(),
                diContainer.Resolve<IOutputWriter>());
        }

        internal static int Run(
            string[] args,
            ITestRunnerFactory testRunnerFactory,
            Func<ITestRunner, IEventListener, ICoverageAnalysisResult, IMutationTestRunner> mutationTestRunnerFactory,
            Func<IEventListener, ICoverageAnalyser> coverageAnalyserFactory,
            ISourceControlIntegration sourceControlIntegration,
            IOutputWriter outputWriter)
        {
            try
            {
                outputWriter.WriteLine($"Fettle v{AssemblyVersionInformation.AssemblyVersion}");

                var parsedArgs = CommandLineArguments.Parse(args, outputWriter);
                if (!parsedArgs.Success)
                {
                    return ExitCodes.ConfigOrArgsAreInvalid;
                }

                var validationErrors = parsedArgs.Config.Validate().ToList();
                if (validationErrors.Any())
                {
                    OutputValidationErrors(validationErrors, outputWriter);
                    return ExitCodes.ConfigOrArgsAreInvalid;
                }

                if (!string.IsNullOrEmpty(parsedArgs.Config.CustomTestRunnerCommand) &&
                    !parsedArgs.ConsoleOptions.SkipCoverageAnalysis)
                {
                    WarnThatOptionsAreIncompatibleWithCoverageAnalysis(outputWriter);
                }

                if (parsedArgs.ConsoleOptions.ModificationsOnly)
                {
                    var result = FindLocallyModifiedSourceFiles(sourceControlIntegration, parsedArgs.Config, outputWriter);
                    if (!result.Success)
                    {
                        outputWriter.WriteFailureLine("Failed to find local modifications.");
                        return ExitCodes.UnexpectedError;
                    }

                    parsedArgs.Config.LocallyModifiedSourceFiles = result.Files;
                }

                if (!parsedArgs.Config.HasAnyMutatableDocuments().Result)
                {
                    outputWriter.WriteLine("No source files found to mutate (or none matched the filters), exiting.");
                    return ExitCodes.Success;
                }

                var eventListener = CreateEventListener(outputWriter, isQuietModeEnabled: parsedArgs.ConsoleOptions.Quiet);

                ICoverageAnalysisResult coverageResult = null;
                var shouldDoCoverageAnalysis = !parsedArgs.Config.HasCustomTestRunnerCommand && !parsedArgs.ConsoleOptions.SkipCoverageAnalysis;
                if (shouldDoCoverageAnalysis)
                {
                    var analyser = coverageAnalyserFactory(eventListener);
                    coverageResult = AnalyseCoverage(analyser, outputWriter, parsedArgs.Config);

                    if (!coverageResult.WasSuccessful)
                    {
                        OutputCoverageAnalysisError(coverageResult.ErrorDescription, outputWriter);
                        return ExitCodes.ConfigOrArgsAreInvalid;
                    }
                }

                var testRunner = parsedArgs.Config.HasCustomTestRunnerCommand ? testRunnerFactory.CreateCustomTestRunner() : testRunnerFactory.CreateNUnitTestRunner();
                var mutationTestRunner = mutationTestRunnerFactory(testRunner, eventListener, coverageResult);
                var mutationTestResult = PerformMutationTesting(mutationTestRunner, parsedArgs.Config, outputWriter);
                if (mutationTestResult.Errors.Any())
                {
                    outputWriter.WriteFailureLine("Unable to perform mutation testing:");
                    mutationTestResult.Errors.ToList().ForEach(e => outputWriter.WriteFailureLine($"==> {e}"));
                    {
                        return ExitCodes.ConfigOrArgsAreInvalid;
                    }
                }

                if (mutationTestResult.SurvivingMutants.Any())
                {
                    OutputAllSurvivorInfo(mutationTestResult.SurvivingMutants, outputWriter, parsedArgs.Config);
                    return ExitCodes.SomeMutantsSurvived;
                }
                else
                {
                    outputWriter.WriteSuccessLine("No mutants survived.");
                    return ExitCodes.Success;
                }
            }
            catch (Exception ex)
            {
                outputWriter.WriteFailureLine($"An error ocurred that Fettle didn't expect.{Environment.NewLine}{ex}");
                return ExitCodes.UnexpectedError;
            }
        }

        private static void WarnThatOptionsAreIncompatibleWithCoverageAnalysis(IOutputWriter outputWriter)
        {
            outputWriter.WriteWarningLine(@"Warning: you've specified a custom test runner command which means coverage analysis cannot be performed. This will likely make mutation testing take longer.
Remove this warning by adding the --skipcoverageanalysis command-line option.
More info at: https://github.com/ComparetheMarket/fettle/wiki/Coverage-Analysis
");
        }

        private static (bool Success, string[] Files) FindLocallyModifiedSourceFiles(
            ISourceControlIntegration sourceControlIntegration, 
            Config config,
            IOutputWriter outputWriter)
        {
            try
            {
                outputWriter.WriteLine("Finding local modifications...");

                var files = sourceControlIntegration.FindLocallyModifiedFiles(config);

                var noun = files.Length == 1 ? "change" : "changes";
                outputWriter.WriteLine($"Found {files.Length} relevant {noun}.");

                return (true, files);
            }
            catch (SourceControlIntegrationException ex)
            {
                outputWriter.WriteFailureLine(ex.Message);
                return (false, null);
            }
        }

        private static IEventListener CreateEventListener(IOutputWriter outputWriter, bool isQuietModeEnabled)
        {
            return isQuietModeEnabled ? (IEventListener) new QuietEventListener(outputWriter)
                                      : (IEventListener) new VerboseEventListener(outputWriter);
        }

        private static ICoverageAnalysisResult AnalyseCoverage(
            ICoverageAnalyser coverageAnalyser, 
            IOutputWriter outputWriter,
            Config config)
        {
            outputWriter.WriteLine("Test coverage analysis starting...");

            var result = coverageAnalyser.AnalyseCoverage(config).Result;

            outputWriter.WriteLine($"Test coverage analysis complete.");

            return result;
        }

        private static MutationTestResult PerformMutationTesting(
            IMutationTestRunner mutationTestRunner, 
            Config config, 
            IOutputWriter outputWriter)
        {
            outputWriter.WriteLine("Mutation testing starting...");
            
            var result = mutationTestRunner.Run(config).Result;
            
            outputWriter.WriteLine("Mutation testing complete.");

            return result;
        }

        private static void OutputValidationErrors(List<string> validationErrors, IOutputWriter outputWriter)
        {
            outputWriter.WriteFailureLine("Validation of configuration failed, check your config file for errors.");
            validationErrors.ForEach(e => outputWriter.WriteFailureLine($"==> {e}"));
        }

        private static void OutputCoverageAnalysisError(string errorDescription, IOutputWriter outputWriter)
        {
            outputWriter.WriteFailureLine("Unable to perform test coverage analysis:");
            outputWriter.WriteFailureLine(errorDescription);
        }

        private static void OutputAllSurvivorInfo(
            IReadOnlyCollection<SurvivingMutant> survivingMutants,
            IOutputWriter outputWriter, 
            Config config)
        {
            outputWriter.WriteFailureLine($"{survivingMutants.Count} mutant(s) survived!");

            survivingMutants
                .Select((sm, index) => new {sm, index})
                .ToList()
                .ForEach(item => OutputSurvivorInfo(item.sm, item.index, config, outputWriter));
        }
        
        private static void OutputSurvivorInfo(SurvivingMutant survivor, int index, Config config, IOutputWriter outputWriter)
        {
            string ToRelativePath(string filePath)
            {
                var baseSourceDir = Path.GetDirectoryName(Path.GetFullPath(config.SolutionFilePath));
                return filePath.Substring(baseSourceDir.Length);
            }

            outputWriter.WriteLine("");
            outputWriter.WriteFailureLine($"({index+1}) {ToRelativePath(survivor.SourceFilePath)}:{survivor.SourceLine}");
            outputWriter.WriteFailureLine($"  original: {survivor.OriginalLine.Trim()}");
            outputWriter.WriteFailureLine($"  mutated: {survivor.MutatedLine.Trim()}");
        }
    }
}
