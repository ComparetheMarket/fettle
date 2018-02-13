using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Fettle.Tests.Console.EndToEnd
{
    [TestFixture]
    public class Happy_path
    {
        [Test]
        public void Console_executable_runs_and_outputs_info_about_surviving_mutants()
        {
            var baseDir = TestContext.CurrentContext.TestDirectory;
            var configFilePath = Path.Combine(baseDir, "Console", "fettle.config.yml");
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

            stopwatch.Stop();
            System.Console.WriteLine($"Duration: {stopwatch.Elapsed.TotalSeconds} seconds");

            var stdOut = fettleProcess.StandardOutput.ReadToEnd();
            System.Console.WriteLine($"*** stdout: {stdOut}");

            var stdErr = fettleProcess.StandardError.ReadToEnd();
            System.Console.WriteLine($"*** stderr: {stdErr}");
            
            Assert.That(fettleProcess.ExitCode, Is.EqualTo(1));
            Assert.That(stdErr, Is.Empty);            
            Assert.That(stdOut, Does.Contain("mutant(s) survived!"));
            Assert.That(stdOut, Does.Contain("PartiallyTestedNumberComparison.cs:7"));
        }

        private static void ModifyConfigFile(string configFilePath)
        {
            var originalConfigFileContents = File.ReadAllText(configFilePath);
            File.WriteAllText(configFilePath, 
                string.Format(originalConfigFileContents, BuildConfig.AsString));
        }
    }
}
