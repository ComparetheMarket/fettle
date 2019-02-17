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
                FileName = "cmd",
                Arguments = fullCommand,

                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            process.WaitForExit();

            return new TestRunResult
            {
                Status = ExitCodeToTestRunStatus(process.ExitCode)
            };
        }

        private static TestRunStatus ExitCodeToTestRunStatus(int exitCode)
        {
            switch (exitCode)
            {
                case 0: return TestRunStatus.AllTestsPassed;
                case 1: return TestRunStatus.SomeTestsFailed;
                default: throw new ApplicationException("");
            }
        }

        public TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            throw new System.NotImplementedException();
        }
    }
}
