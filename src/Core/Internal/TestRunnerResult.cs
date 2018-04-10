namespace Fettle.Core.Internal
{
    internal enum TestRunStatus
    {
        AllTestsPassed,
        SomeTestsFailed
    }

    internal class TestRunResult
    {
        public TestRunStatus Status { get; set; }
        public string Error { get; set; }
        public string ConsoleOutput { get; set; }
    }
}