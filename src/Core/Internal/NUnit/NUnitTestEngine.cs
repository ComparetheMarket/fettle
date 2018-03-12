using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
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
            IDictionary<string, string> methodIdsToNames,
            Action<string, int> onAnalysingTestCase,
            IDictionary<string, ImmutableHashSet<string>> methodsAndCoveringTests)
        {
            var numTestCasesExecuted = 0;

            var testEventListener = new EventListener(
                onTestStarting: testMethodName =>
                {
                    onAnalysingTestCase(testMethodName, numTestCasesExecuted);
                    numTestCasesExecuted++;
                },
                onTestComplete: (testMethodName, executedMethodIds) =>
                {
                    foreach (var executedMethodId in executedMethodIds)
                    {
                        var executedMethodName = methodIdsToNames[executedMethodId];

                        if (!methodsAndCoveringTests.ContainsKey(executedMethodName))
                            methodsAndCoveringTests.Add(executedMethodName, ImmutableHashSet<string>.Empty);

                        methodsAndCoveringTests[executedMethodName] =
                            methodsAndCoveringTests[executedMethodName].Add(testMethodName);
                    }
                },
                onTestFixtureComplete: (testMethodsInFixture, executedMethodIds) =>
                {
                    foreach (var executedMethodId in executedMethodIds)
                    {
                        var executedMethodName = methodIdsToNames[executedMethodId];

                        if (!methodsAndCoveringTests.ContainsKey(executedMethodName))
                            methodsAndCoveringTests.Add(executedMethodName, ImmutableHashSet<string>.Empty);

                        foreach (var testMethodName in testMethodsInFixture)
                        {
                            methodsAndCoveringTests[executedMethodName] =
                                methodsAndCoveringTests[executedMethodName].Add(testMethodName);
                        }
                    }
                });

            return RunTests(testAssemblyFilePaths, testMethodNames, testEventListener);
        }

        public TestRunResult RunTests(IEnumerable<string> testAssemblyFilePaths, IEnumerable<string> testMethodNames)
        {
            return RunTests(testAssemblyFilePaths, testMethodNames, new NullEventListener());
        }
        
        private TestRunResult RunTests(
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
                // implementation of ITestEventListener when running tests.
            }
        }

        private class EventListener : ITestEventListener
        {
            private readonly Action<string> onTestStarting;
            private readonly Action<string, IEnumerable<string>> onTestComplete;
            private readonly Action<IEnumerable<string>, IEnumerable<string>> onTestFixtureComplete;

            private readonly List<string> testCasesWithinFixture = new List<string>();

            public EventListener(
                Action<string> onTestStarting,
                Action<string, IEnumerable<string>> onTestComplete,
                Action<IEnumerable<string>, IEnumerable<string>> onTestFixtureComplete)
            {
                this.onTestStarting = onTestStarting;
                this.onTestComplete = onTestComplete;
                this.onTestFixtureComplete = onTestFixtureComplete;
            }

            public void OnTestEvent(string report)
            {
                if (report.StartsWith("<start-test"))
                {
                    var doc = XDocument.Parse(report);

                    var testName = doc.Root.Attribute("fullname").Value;

                    onTestStarting(testName);
                }
                else if (report.StartsWith("<test-case"))
                {
                    var doc = XDocument.Parse(report);

                    var testName = doc.Root.Attribute("fullname").Value;
                    testCasesWithinFixture.Add(testName);

                    var calledMethodIds = ParseExecutedMethodIdsFromOutput(doc);

                    onTestComplete(testName, calledMethodIds);
                }
                else if (report.StartsWith("<test-suite"))
                {
                    var doc = XDocument.Parse(report);

                    var calledMethodIds = ParseExecutedMethodIdsFromOutput(doc);
                    onTestFixtureComplete(testCasesWithinFixture, calledMethodIds);

                    testCasesWithinFixture.Clear();
                }
            }

            private static List<string> ParseExecutedMethodIdsFromOutput(XDocument doc)
            {
                var consoleOutput = new StringBuilder();
                foreach (var outputNode in doc.XPathSelectElements("//test-case/output|//test-suite/output"))
                {
                    consoleOutput.Append(outputNode.Value);
                }

                var calledMethodIds = new List<string>();
                foreach (var outputLine in consoleOutput
                    .ToString()
                    .Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    const string prefix = "fettle_covered_method:";
                    if (outputLine.StartsWith(prefix))
                    {
                        var method = outputLine.Substring(prefix.Length);
                        calledMethodIds.Add(method);
                    }
                }

                return calledMethodIds;
            }
        }
    }
}