using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Fettle.Core;

namespace Fettle.Console
{
    internal static class CommandLineArguments
    {
        private class Arguments
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local

            [Option('c', "config", Required = true, 
                HelpText = "Path to configuration file")]
            public string ConfigFilePath { get; set; }

            [Option('q', "quiet", Required = false, DefaultValue = false, 
                HelpText = "If specified, less output will be produced")]
            public bool Quiet { get; set; }

            [Option('v', "verbose", Required = false, DefaultValue = false,
                HelpText = "If specified, detailed output will be produced")]
            public bool Verbose { get; set; }

            [Option('s', "skipcoverageanalysis", Required = false, DefaultValue = false,
                HelpText = "If specified, the coverage analysis optimisation is skipped.")]
            public bool SkipCoverageAnalysis { get; set; }

            [Option('m', "modificationsonly", Required = false, DefaultValue = false,
                HelpText = "If specified, only the .cs files you have changed locally will be considered for mutation. This requires your source-code to be within a git repository.")]
            public bool ModificationsOnly { get; set; }

            // ReSharper restore UnusedAutoPropertyAccessor.Local

            [HelpOption]
            // ReSharper disable once UnusedMember.Local
            public string GetUsage()
            {
                var help = new HelpText
                {
                    AddDashesToOption = true,
                    AdditionalNewLineAfterOption = true
                };
                help.AddPreOptionsLine("Usage: Fettle.Console.exe --config <file> [options]");
                help.AddOptions(this);
                return help;
            }
        }

        public static (bool Success, Config Config, ConsoleOptions ConsoleOptions) Parse(
            string[] args,
            IOutputWriter outputWriter)
        {
            var parsedArgs = new Arguments();
            if (!Parser.Default.ParseArguments(args, parsedArgs))
            {
                if (args.All(a => a != "--help"))
                {
                    outputWriter.WriteLine("");
                    outputWriter.WriteFailureLine("Invalid arguments specified.");
                }
                return (false, null, null);
            }
            
            var configFilePath = parsedArgs.ConfigFilePath;
            if (!File.Exists(configFilePath))
            {
                outputWriter.WriteFailureLine($"Failed to find config file \"{configFilePath}\"");
                return (false, null, null);
            }

            if (parsedArgs.Quiet && parsedArgs.Verbose)
            {
                outputWriter.WriteFailureLine("Both quiet and verbose options were specified, but only one or the other is allowed.");
                return (false, null, null);
            }

            var configFileContents = File.ReadAllText(configFilePath);
            var config = ConfigFile.Parse(configFileContents)
                .WithPathsRelativeTo(baseDirectory: Path.GetDirectoryName(configFilePath));
            
            var consoleOptions = new ConsoleOptions
            {
                Verbosity = ParsedArgsToVerbosity(parsedArgs),
                SkipCoverageAnalysis = parsedArgs.SkipCoverageAnalysis,
                ModificationsOnly = parsedArgs.ModificationsOnly,
            };

            return (true, config, consoleOptions);
        }

        private static Verbosity ParsedArgsToVerbosity(Arguments parsedArgs)
        {
            if (parsedArgs.Quiet)
            {
                return Verbosity.Quiet;
            }

            if (parsedArgs.Verbose)
            {
                return Verbosity.Verbose;
            }

            return Verbosity.Default;
        }
    }
}