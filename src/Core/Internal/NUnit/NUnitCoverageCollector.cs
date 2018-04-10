using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using NUnit.Engine;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitCoverageCollector : ITestEventListener
    {
        private readonly IDictionary<string, string> methodIdsToNames;
        private readonly Action<string, int> onAnalysingTestCase;
        private int numTestCasesExecuted;
        private NUnitEventListener TestEventListener { get; }
            
        private readonly Dictionary<string, ImmutableHashSet<string>> methodsAndCoveringTests 
            = new Dictionary<string, ImmutableHashSet<string>>();

        public IReadOnlyDictionary<string, ImmutableHashSet<string>> MethodsAndCoveringTests { get; }

        public NUnitCoverageCollector(IDictionary<string, string> methodIdsToNames, Action<string, int> onAnalysingTestCase)
        {
            this.methodIdsToNames = methodIdsToNames;
            this.onAnalysingTestCase = onAnalysingTestCase;

            MethodsAndCoveringTests = new ReadOnlyDictionary<string, ImmutableHashSet<string>>(methodsAndCoveringTests);
            TestEventListener = new NUnitEventListener(OnTestStarting, OnTestComplete, OnTestFixtureComplete);
        }

        public void OnTestEvent(string report)
        {
            TestEventListener.OnTestEvent(report);
        }

        private void OnTestFixtureComplete(IEnumerable<string> testMethodsInFixture, IEnumerable<string> executedMethodIds)
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
        }

        private void OnTestStarting(string testMethodName)
        {
            onAnalysingTestCase(testMethodName, numTestCasesExecuted);
            numTestCasesExecuted++;
        }

        private void OnTestComplete(string testMethodName, IEnumerable<string> executedMethodIds)
        {
            foreach (var executedMethodId in executedMethodIds)
            {
                var executedMethodName = methodIdsToNames[executedMethodId];

                if (!methodsAndCoveringTests.ContainsKey(executedMethodName))
                    methodsAndCoveringTests.Add(executedMethodName, ImmutableHashSet<string>.Empty);

                methodsAndCoveringTests[executedMethodName] =
                    methodsAndCoveringTests[executedMethodName].Add(testMethodName);
            }
        }
    }
}