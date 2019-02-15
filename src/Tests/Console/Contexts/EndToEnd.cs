using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace Fettle.Tests.Console.Contexts
{
    public class EndToEnd
    {
        protected int ExitCode { get; private set; }
        protected string StandardOutput { get; private set; }
        protected string StandardError { get; private set; }

        protected void When_running_real_fettle_console_executable(string configFilename)
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            var configFilePath = Path.Combine(baseDir, "Console", configFilename);
            ModifyConfigFile(configFilePath);

            var fettleProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = Path.Combine(baseDir, "Fettle.Console.exe"),
                    Arguments = $"--quiet --config {configFilePath}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            var stopwatch = Stopwatch.StartNew();

            fettleProcess.Start();
            fettleProcess.WaitForExit((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

            ExitCode = fettleProcess.ExitCode;
            StandardOutput = fettleProcess.StandardOutput.ReadToEnd();
            StandardError = fettleProcess.StandardError.ReadToEnd();

            stopwatch.Stop();
        }

        private static void ModifyConfigFile(string configFilePath)
        {
            var originalConfigFileContents = File.ReadAllText(configFilePath);
            File.WriteAllText(configFilePath,
                string.Format(originalConfigFileContents, BuildConfig.AsString));
        }
    }
}