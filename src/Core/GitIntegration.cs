using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Fettle.Core
{
    public class GitStatusException : Exception
    {
        public GitStatusException(string output) : base(Description(output))
        {
        }

        private static string Description(string output) =>
            $"'git status' command failed.{Environment.NewLine}Output:{Environment.NewLine}{output}";
    }

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

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c git status --porcelain=2",
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
                throw new GitStatusException(output);
            }

            return output;
        }

        private static string[] ParseGitStatusOutput(string output)
        {
            bool IsPathWithinSolutionDirectory(string filePath) => !filePath.StartsWith("..");
            
            return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(x => x.Split(' ').Last())
                                     .Where(x => IsPathWithinSolutionDirectory(x))
                                     .Select(x => x.Replace('/', '\\'))
                                     .ToArray();
        }
    }
}
