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

            [Option('c', "config", Required = true, HelpText = "Path to configuration file")]
            public string ConfigFilePath { get; set; }

            [Option('q', "quiet", Required = false, HelpText = "If specified, less output will be produced", DefaultValue = false)]
            public bool Quiet { get; set; }
            
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            [HelpOption]
            // ReSharper disable once UnusedMember.Local
            public string GetUsage()
            {
                var help = new HelpText
                {
                    AddDashesToOption = true
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

            var configFileContents = File.ReadAllText(configFilePath);
            var config = ConfigFile.Parse(configFileContents)
                .WithPathsRelativeTo(baseDirectory: Path.GetDirectoryName(configFilePath));
            
            var consoleOptions = new ConsoleOptions{ Quiet = parsedArgs.Quiet };

            return (true, config, consoleOptions);
        }
    }
}