using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Fettle.Core
{
    public class GitIntegration : ISourceControlIntegration
    {
        public string[] FindLocallyModifiedFiles(Config config)
        {
            var output = GitStatusOutput(config);
            return ParseGitStatusOutput(output);
        }

        private static string GitStatusOutput(Config config)
        {
            var solutionDir = Path.GetFullPath(Path.GetDirectoryName(config.SolutionFilePath));

            const string gitStatusWithParseableOutput = "git status --porcelain=2";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {gitStatusWithParseableOutput}",
                WorkingDirectory = solutionDir,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
            process.Start();
            process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds);

            var output = process.StandardOutput.ReadToEnd();
            if (process.ExitCode != 0)
            {
                var message = $"'git status' command failed.{Environment.NewLine}Output:{Environment.NewLine}{output}";
                throw new SourceControlIntegrationException(message);
            }

            return output;
        }

        private static string[] ParseGitStatusOutput(string output)
        {
            bool IsPathWithinSolutionDirectory(string filePath) => !filePath.StartsWith("..");
            
            return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(x => x.Split(' ').Last())
                                     .Where(IsPathWithinSolutionDirectory)
                                     .Where(x => x.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase))
                                     .Select(x => x.Replace('/', '\\'))
                                     .ToArray();
        }
    }
}
