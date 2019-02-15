using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;

namespace Fettle.Core
{
    public static class TestRunnerFactory
    {
        public static ITestRunner CreateNUnitTestRunner() => new NUnitTestRunner();

        public static ITestRunner CreateCustomTestRunner() => new CustomTestRunner();
    }
}
