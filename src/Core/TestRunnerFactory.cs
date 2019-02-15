using Fettle.Core.Internal.NUnit;

namespace Fettle.Core
{
    public static class TestRunnerFactory
    {
        public static ITestRunner CreateNUnitTestRunner()
        {
            return new NUnitTestRunner();
        }
    }
}
