namespace Fettle.Core
{
    public enum TestRunStatus
    {
        AllTestsPassed,
        SomeTestsFailed
    }

    public class TestRunResult
    {
        public TestRunStatus Status { get; set; }
        public string Error { get; set; }
    }
}