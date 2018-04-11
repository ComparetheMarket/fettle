using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Fettle.Core;

namespace Fettle.Console
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            IMutationTestRunner CreateRealMutationTestRunner(
                IEventListener eventListener,
                CoverageAnalysisResult coverageAnalysisResult)
            {
                return new MutationTestRunner(
                    coverageAnalysisResult, 
                    eventListener);
            }

            ICoverageAnalyser CreateRealCoverageAnalyser(IEventListener eventListener)
            {
                return new CoverageAnalyser(eventListener);
            }
            
            return InternalEntryPoint(
                args, 
                CreateRealMutationTestRunner, 
                CreateRealCoverageAnalyser, 
                new ConsoleOutputWriter());
        }
        
        private static class ExitCodes
        {
            public const int NoMutantsSurvived = 0;
            public const int SomeMutantsSurvived = 1;
            public const int ConfigOrArgsAreInvalid = 2;
            public const int UnexpectedError = 3;
        }
        
        internal static int InternalEntryPoint(
            string[] args,
            Func<IEventListener, CoverageAnalysisResult, IMutationTestRunner> mutationTestRunnerFactory,
            Func<IEventListener, ICoverageAnalyser> coverageAnalyserFactory,
            IOutputWriter outputWriter)
        {
            try
            {
                var parsedArgs = CommandLineArguments.Parse(args, outputWriter);
                if (!parsedArgs.Success)
                {
                    return ExitCodes.ConfigOrArgsAreInvalid;
                }
                
                var eventListener = parsedArgs.ConsoleOptions.Quiet
                    ? (IEventListener) new QuietEventListener(outputWriter)
                    : (IEventListener) new VerboseEventListener(outputWriter);

                var analyser = coverageAnalyserFactory(eventListener);
                var coverageResult = AnalyseCoverage(analyser, outputWriter, parsedArgs.Config);
                if (coverageResult.ErrorDescription != null)
                {
                    return ExitCodes.ConfigOrArgsAreInvalid;
                }

                outputWriter.WriteLine("Mutation testing starting...");
                var runner = mutationTestRunnerFactory(eventListener, coverageResult);
                var result = runner.Run(parsedArgs.Config).Result;
                
                if (result.Errors.Any())
                {
                    outputWriter.WriteFailureLine("Unable to perform mutation testing:");
                    result.Errors.ToList().ForEach(e => outputWriter.WriteFailureLine($"==> {e}"));
                    return ExitCodes.ConfigOrArgsAreInvalid;
                }

                outputWriter.Write(Environment.NewLine + Environment.NewLine);
                outputWriter.WriteLine("Mutation testing complete.");

                if (result.SurvivingMutants.Any())
                {
                    outputWriter.WriteFailureLine($"{result.SurvivingMutants.Count} mutant(s) survived!");

                    result.SurvivingMutants
                        .Select((sm, index) => new {sm, index})
                        .ToList()
                        .ForEach(item => OutputSurvivorInfo(item.sm, item.index, parsedArgs.Config, outputWriter));

                    return ExitCodes.SomeMutantsSurvived;
                }
                else
                {
                    outputWriter.WriteSuccessLine("No mutants survived.");
                    return ExitCodes.NoMutantsSurvived;
                }
            }
            catch (Exception ex)
            {
                outputWriter.WriteFailureLine($"An error ocurred that Fettle didn't expect.{Environment.NewLine}{ex}");
                return ExitCodes.UnexpectedError;
            }
        }

        private static CoverageAnalysisResult AnalyseCoverage(
            ICoverageAnalyser coverageAnalyser, 
            IOutputWriter outputWriter,
            Config config)
        {
            outputWriter.Write("Analysing test coverage");

            var coverageResult = coverageAnalyser.AnalyseMethodCoverage(config).Result;
            if (coverageResult.ErrorDescription != null)
            {
                outputWriter.WriteFailureLine("Unable to perform test coverage analysis:");
                outputWriter.WriteFailureLine(coverageResult.ErrorDescription);
            }
            else
            {
                outputWriter.Write(Environment.NewLine);
            }

            return coverageResult;
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
