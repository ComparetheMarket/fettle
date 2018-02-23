using System.Collections.Generic;
using System.Linq;
using NUnit.Engine;
using NUnit.Engine.Services;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitTestEngine : ITestFinder, ITestRunner
    {
        public string[] FindTests(IEnumerable<string> testAssemblyFilePaths)
        {
            using (var testEngine = CreateTestEngine())
            {
                var testPackage = CreateTestPackage(testAssemblyFilePaths);

                using (var testRunner = testEngine.GetRunner(testPackage))
                {
                    var results = testRunner.Explore(TestFilter.Empty);
                    return NUnitExploreResults.Parse(results);
                }
            }
        }

        public TestRunnerResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            using (var testEngine = CreateTestEngine())
            {
                var testPackage = CreateTestPackage(testAssemblyFilePaths);

                using (var testRunner = testEngine.GetRunner(testPackage))
                {
                    var filterBuilder = new TestFilterBuilder();
                    testMethodNames.ToList().ForEach(tn => 
                        filterBuilder.AddTest(RemoveReturnTypeAndParameters(tn)));

                    var results = testRunner.Run(new NullEventListener(), filterBuilder.GetFilter());
                    return NUnitRunResults.Parse(results);
                }
            }
        }

        private string RemoveReturnTypeAndParameters(string fullMethodName)
        {
            var nameWithoutReturnType = fullMethodName.Split(' ').Skip(1).First();
            var nameWithoutParams = nameWithoutReturnType.Split('(').First();
            return nameWithoutParams.Replace("::", ".");
        }

        private static TestEngine CreateTestEngine()
        {
            var testEngine = new TestEngine();

            testEngine.Services.Add(new SettingsService(false));
            testEngine.Services.Add(new ExtensionService());
            testEngine.Services.Add(new InProcessTestRunnerFactory());
            testEngine.Services.Add(new DomainManager());
            testEngine.Services.Add(new DriverService());
            testEngine.Services.Add(new TestFilterService());
            testEngine.Services.Add(new ProjectService());
            testEngine.Services.Add(new RuntimeFrameworkService());

            testEngine.Services.ServiceManager.StartServices();

            return testEngine;
        }

        private static TestPackage CreateTestPackage(IEnumerable<string> testAssemblyFilePaths)
        {
            var testPackage = new TestPackage(testAssemblyFilePaths.ToList());
            testPackage.AddSetting("StopOnError", true);
            testPackage.AddSetting("ShadowCopyFiles", true);
            testPackage.AddSetting("DomainUsage", "Single");
            testPackage.AddSetting("ProcessModel", "Separate");
            return testPackage;
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