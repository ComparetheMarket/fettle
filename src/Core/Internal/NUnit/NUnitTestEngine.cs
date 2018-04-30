using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Engine;
using NUnit.Engine.Services;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitTestEngine : ITestFinder, ITestRunner
    {
        static NUnitTestEngine()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Console.WriteLine($"{args.Name}....");
                return null;
            };
        }

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

                SpikeRunTests(testAssemblyFilePaths.First());
                var results = testRunner.Run(testEventListener, filterBuilder.GetFilter());
                return NUnitRunResults.Parse(results);
            }
        }

        private static void SpikeRunTests(string filePath)
        {
            //    var testAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(filePath);
            //    var dependencyContext = DependencyContext.Load(testAssembly);
            //    var loadContext = AssemblyLoadContext.GetLoadContext(testAssembly);
            //    var assemblyResolver = new CompositeCompilationAssemblyResolver(
            //        new ICompilationAssemblyResolver[]
            //        {
            //            //new AppBaseCompilationAssemblyResolver(@"C:\dev\spikes\dnc-nunit\service\bin\Debug\netcoreapp2.0"),
            //            new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(filePath)),
            //            new ReferenceAssemblyPathResolver(),
            //            new PackageCompilationAssemblyResolver()
            //        });
            //    AssemblyLoadContext.Default.Resolving += (context, name) =>
            //    {
            //        var finalName = name.Name == "nunit.framework" ? "NUnit" : name.Name;

            //        var library = dependencyContext.RuntimeLibraries.FirstOrDefault(l =>
            //            string.Equals(l.Name, finalName, StringComparison.OrdinalIgnoreCase));

            //        if (library != null)
            //        {
            //            var compilationLibrary = new CompilationLibrary(
            //                library.Type,
            //                library.Name,
            //                library.Version,
            //                library.Hash,
            //                library.RuntimeAssemblyGroups.SelectMany(x => x.AssetPaths),
            //                library.Dependencies,
            //                library.Serviceable);

            //            var resolvedAssemblyPaths = new List<string>();
            //            assemblyResolver.TryResolveAssemblyPaths(compilationLibrary, resolvedAssemblyPaths);
            //            if (resolvedAssemblyPaths.Any())
            //            {
            //                return loadContext.LoadFromAssemblyPath(resolvedAssemblyPaths.First());
            //            }
            //        }

            //        return null;
            //    };
        }
        
        private static TestEngine CreateTestEngine()
        {
            var result = new TestEngine();
            result.InternalTraceLevel = InternalTraceLevel.Debug;

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
            testPackage.AddSetting("ProcessModel", "InProcess");
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