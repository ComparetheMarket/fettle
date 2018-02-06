using System;
using System.IO;
using System.Linq;
using Fettle.Core;

namespace Fettle.Console
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            var mutationTestRunnerFactory = new Func<IEventListener, IMutationTestRunner>(
                el => new MutationTestRunner(el));

            return InternalEntryPoint(args, mutationTestRunnerFactory, new ConsoleOutputWriter());
        }

        internal static int InternalEntryPoint(
            string[] args,
            Func<IEventListener, IMutationTestRunner> mutationTestRunnerFactory,
            IOutputWriter outputWriter)
        {
            try
            {
                var parsedArgs = CommandLineArguments.Parse(args, outputWriter);
                if (!parsedArgs.Success)
                {
                    return 2;
                }

                var eventListener = parsedArgs.ConsoleOptions.Quiet ?
                    (IEventListener)new QuietEventListener(outputWriter) : 
                    (IEventListener)new VerboseEventListener(outputWriter);

                var runner = mutationTestRunnerFactory(eventListener);

                var result = runner.Run(config: parsedArgs.Config)
                    .Result;

                outputWriter.Write(Environment.NewLine + Environment.NewLine);
                outputWriter.WriteLine("Mutation testing complete.");
                
                if (result.SurvivingMutants.Any())
                {
                    outputWriter.WriteFailureLine($"{result.SurvivingMutants.Count} mutant(s) survived!");
                    
                    result.SurvivingMutants
                        .Select((sm, index) => new { sm, index })
                        .ToList()
                        .ForEach(item => OutputSurvivorInfo(item.sm, item.index, parsedArgs.Config, outputWriter));

                    return 1;
                }
                else
                {
                    outputWriter.WriteSuccessLine("No mutants survived.");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                outputWriter.WriteFailureLine($"An error ocurred that Fettle didn't expect.{Environment.NewLine}{ex}");
                return 3;
            }
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
