namespace Fettle.Core
{
    public interface ITestRunnerFactory
    {
        ITestRunner CreateNUnitTestRunner();

        ITestRunner CreateCustomTestRunner(string testRunnerCommand);
    }
}
