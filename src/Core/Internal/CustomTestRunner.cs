using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fettle.Core.Internal
{
    internal class CustomTestRunner : ITestRunner
    {
        private readonly string testRunnerCommand;

        public CustomTestRunner(string testRunnerCommand)
        {
            this.testRunnerCommand = testRunnerCommand;
        }

        public TestRunResult RunAllTests(IEnumerable<string> testAssemblyFilePaths)
        {
            var fullCommand = $"/c {testRunnerCommand} {string.Join(" ", testAssemblyFilePaths)}";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = fullCommand,

                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            process.WaitForExit();

            return CompletedProcessToTestRunResult(process);
        }

        private TestRunResult CompletedProcessToTestRunResult(Process process)
        {
            switch (process.ExitCode)
            {
                case 0:
                    return new TestRunResult { Status = TestRunStatus.AllTestsPassed };
                case 1:
                    return new TestRunResult { Status = TestRunStatus.SomeTestsFailed };
                default:
                    throw new InvalidOperationException(
$@"Custom test runner returned an unexpected exit code: {process.ExitCode}.
StdOut: {process.StandardOutput.ReadToEnd()}
StdErr: {process.StandardError.ReadToEnd()}");
            }
        }

        public TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            throw new System.NotImplementedException();
        }
    }
}
