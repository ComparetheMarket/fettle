using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Fettle.Core.Internal
{
    internal class CustomTestRunner : ITestRunner
    {
        private readonly string testRunnerCommand;
        private readonly string baseDirectory;

        public CustomTestRunner(string testRunnerCommand, string baseDirectory)
        {
            this.testRunnerCommand = testRunnerCommand;
            this.baseDirectory = baseDirectory;
        }

        public TestRunResult RunAllTests(IEnumerable<string> testAssemblyFilePaths)
        {
            var fullCommand = $"/c {testRunnerCommand} {string.Join(" ", testAssemblyFilePaths)}";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = fullCommand,
                WorkingDirectory = baseDirectory,

                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            var capturedStdOut = new StringBuilder();
            var capturedStdErr = new StringBuilder();
            process.OutputDataReceived += (sender, args) => capturedStdOut.Append(args.Data);
            process.ErrorDataReceived += (sender, args) => capturedStdErr.Append(args.Data);

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();

            var exitCode = process.ExitCode;
            process.Close();

            return CompletedProcessToTestRunResult(exitCode, capturedStdOut.ToString(), capturedStdErr.ToString());
        }

        private TestRunResult CompletedProcessToTestRunResult(int exitCode, string stdOut, string stdErr)
        {
            switch (exitCode)
            {
                case 0:
                    return new TestRunResult
                    {
                        Status = TestRunStatus.AllTestsPassed
                    };
                case 1:
                    return new TestRunResult
                    {
                        Status = TestRunStatus.SomeTestsFailed,
                        Error = FormattedOutput(stdOut, stdErr)
                    };
                default:
                    throw new InvalidOperationException(
                        $@"Custom test runner returned an unexpected exit code: {exitCode}.{FormattedOutput(stdOut, stdErr)}");
            }
        }

        private static string FormattedOutput(string stdOut, string stdErr)
        {
            return 
$@"StdOut: {stdOut}
StdErr: {stdErr}";
        }

        public TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            throw new System.NotImplementedException();
        }
    }
}
