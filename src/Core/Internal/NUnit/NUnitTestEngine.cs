using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
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

        public TestRunResult RunTestsAndCollectExecutedMethods(
            IEnumerable<string> testAssemblyFilePaths,
            IEnumerable<string> testMethodNames,
            IDictionary<string, ImmutableHashSet<string>> methodsAndCoveringTests)
        {
            var collector = new CoverageCollectingServer();
            collector.Start();

            try
            {
                var testEventListener = new EventListener(
                    onTestComplete: testMethodName =>
                    {
                        var executed = collector.PopReceived();
                        //Console.WriteLine($"...{testMethodName} complete, {executed.Length} exec'd");
                        Console.Write(".");

                        foreach (var executedMethodName in executed)
                        {
                            if (!methodsAndCoveringTests.ContainsKey(executedMethodName))
                                methodsAndCoveringTests.Add(executedMethodName, ImmutableHashSet<string>.Empty);

                            methodsAndCoveringTests[executedMethodName] =
                                methodsAndCoveringTests[executedMethodName].Add(testMethodName);
                        }
                    });

                return RunTests(testAssemblyFilePaths, testMethodNames, testEventListener, runSequentially: true);
            }
            finally
            {
                collector.Stop();
            }
        }

        public TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            return RunTests(testAssemblyFilePaths, testMethodNames, new NullEventListener());
        }
        
        private TestRunResult RunTests(
            IEnumerable<string> testAssemblyFilePaths, 
            IEnumerable<string> testMethodNames,
            ITestEventListener testEventListener,
            bool runSequentially = false)
        {
            using (var testEngine = CreateTestEngine())
            using (var testRunner = testEngine.GetRunner(CreateTestPackage(testAssemblyFilePaths, runSequentially)))
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

        private static TestPackage CreateTestPackage(IEnumerable<string> testAssemblyFilePaths, bool runSequentially = false)
        {
            var testPackage = new TestPackage(testAssemblyFilePaths.ToList());
            
            testPackage.AddSetting("StopOnError", true);
            testPackage.AddSetting("ShadowCopyFiles", true);
            testPackage.AddSetting("DomainUsage", "Single");

            if (runSequentially)
            {
                testPackage.AddSetting("ProcessModel", "Single");
                testPackage.AddSetting("MaxAgents", 1);
                testPackage.AddSetting("NumberOfTestWorkers", 1);
                testPackage.AddSetting("SynchronousEvents", true);
            }
            else
            {
                testPackage.AddSetting("ProcessModel", "Separate");
            }

            return testPackage;
        }

        private class NullEventListener : ITestEventListener
        {
            public void OnTestEvent(string report)
            {
                // We don't need to respond to events, but the NUnit Engine API requires an 
                // implementation of ITestEventListener when running tests.
            }
        }

        private class EventListener : ITestEventListener
        {
            private readonly Action<string> onTestComplete;

            public EventListener(Action<string> onTestComplete)
            {
                this.onTestComplete = onTestComplete;
            }

            public void OnTestEvent(string report)
            {
                if (report.StartsWith("<test-case"))
                {
                    var doc = XDocument.Parse(report);
                    var testName = doc.Root.Attribute("fullname").Value;

                    onTestComplete(testName);
                }
            }
        }
    }
}