using System;
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
            var testPackage = CreateTestPackage(testAssemblyFilePaths);

            using (var testEngine = CreateTestEngine())
            using (var testRunner = testEngine.GetRunner(testPackage))
            {
                var results = testRunner.Explore(TestFilter.Empty);
                return NUnitExploreResults.Parse(results);
            }
        }

        public TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            return RunTests(testAssemblyFilePaths, testMethodNames, new NullEventListener());
        }

        public CoverageTestRunResult RunTestsAndAnalyseCoverage(
            IEnumerable<string> testAssemblyFilePaths,
            IEnumerable<string> testMethodNames,
            IDictionary<string, string> methodIdsToNames,
            Action<string, int> onAnalysingTestCase)
        {
            var coverageCollector = new NUnitCoverageCollector(methodIdsToNames, onAnalysingTestCase);
            
            var runTestsResult = RunTests(testAssemblyFilePaths, testMethodNames, coverageCollector);

            return new CoverageTestRunResult
            {
                Status = runTestsResult.Status,
                Error = runTestsResult.Error,
                ConsoleOutput = runTestsResult.ConsoleOutput,

                MethodsAndCoveringTests = coverageCollector.MethodsAndCoveringTests.ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private static TestRunResult RunTests(
            IEnumerable<string> testAssemblyFilePaths, 
            IEnumerable<string> testMethodNames,
            ITestEventListener testEventListener)
        {
            using (var testEngine = CreateTestEngine())
            using (var testRunner = testEngine.GetRunner(CreateTestPackage(testAssemblyFilePaths)))
            {
                var filterBuilder = new TestFilterBuilder();
                testMethodNames.ToList().ForEach(tn =>
                    filterBuilder.AddTest(tn));

                var results = testRunner.Run(testEventListener, filterBuilder.GetFilter());
                return NUnitRunResults.Parse(results);
            }
        }

        private static TestEngine CreateTestEngine()
        {
            var result = new TestEngine();

            result.Services.Add(new SettingsService(false));
            result.Services.Add(new ExtensionService());
            result.Services.Add(new InProcessTestRunnerFactory());
            result.Services.Add(new DomainManager());
            result.Services.Add(new DriverService());
            result.Services.Add(new TestFilterService());
            result.Services.Add(new ProjectService());
            result.Services.Add(new RuntimeFrameworkService());

            result.Services.ServiceManager.StartServices();

            return result;
        }

        private static TestPackage CreateTestPackage(IEnumerable<string> testAssemblyFilePaths)
        {
            var testPackage = new TestPackage(testAssemblyFilePaths.ToList());
            testPackage.AddSetting("StopOnError", true);
            testPackage.AddSetting("ShadowCopyFiles", false);
            testPackage.AddSetting("DomainUsage", "Single");
            testPackage.AddSetting("ProcessModel", "Separate");
            return testPackage;
        }

        private class NullEventListener : ITestEventListener
        {
            public void OnTestEvent(string report)
            {
                // Sometimes when running tests we don't need to respond to events, but the NUnit Engine API requires an 
                // implementation of ITestEventListener.
            }
        }
    }
}