using System.Collections.Generic;
using System.Linq;
using NUnit.Engine;
using NUnit.Engine.Services;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitTestRunner : ITestRunner
    {
        public TestRunnerResult RunTests(IEnumerable<string> testAssemblyFilePaths)
        {
            using (var testEngine = new TestEngine())
            {
                testEngine.Services.Add(new SettingsService(false));
                testEngine.Services.Add(new ExtensionService());
                testEngine.Services.Add(new InProcessTestRunnerFactory());
                testEngine.Services.Add(new DomainManager());
                testEngine.Services.Add(new DriverService());
                testEngine.Services.Add(new TestFilterService());
                testEngine.Services.Add(new ProjectService());
                testEngine.Services.Add(new RuntimeFrameworkService());

                testEngine.Services.ServiceManager.StartServices();

                var testPackage = new TestPackage(testAssemblyFilePaths.ToList());
                testPackage.AddSetting("StopOnError", true);
                testPackage.AddSetting("ShadowCopyFiles", true);
                testPackage.AddSetting("DomainUsage", "Single");
                testPackage.AddSetting("ProcessModel", "Separate");

                using (var testRunner = testEngine.GetRunner(testPackage))
                {
                    var results = testRunner.Run(new NullEventListener(), TestFilter.Empty);
                    return NUnitRunResults.Parse(results);
                }
            }
        }

        private class NullEventListener : ITestEventListener
        {
            public void OnTestEvent(string report)
            {
                // We don't need to respond to events, but the NUnit Engine API requires an 
                // implementation of ITestEventListener.
            }
        }
    }
}