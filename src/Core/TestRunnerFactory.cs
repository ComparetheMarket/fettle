using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;

namespace Fettle.Core
{
    public class TestRunnerFactory : ITestRunnerFactory
    {
        public ITestRunner CreateNUnitTestRunner() => new NUnitTestRunner();

        public ITestRunner CreateCustomTestRunner(string testRunnerCommand) => new CustomTestRunner(testRunnerCommand);
    }
}
