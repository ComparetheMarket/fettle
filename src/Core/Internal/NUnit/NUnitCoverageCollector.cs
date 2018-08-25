using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Engine;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitCoverageCollector : ITestEventListener
    {
        private readonly IDictionary<string, string> memberIdsToNames;
        private readonly Action<string, int> onAnalysingTestCase;
        private int numTestCasesExecuted;
        private NUnitEventListener TestEventListener { get; }
            
        private readonly Dictionary<string, ImmutableHashSet<string>> membersAndCoveringTests 
            = new Dictionary<string, ImmutableHashSet<string>>();

        public IReadOnlyDictionary<string, ImmutableHashSet<string>> MembersAndCoveringTests { get; }

        public NUnitCoverageCollector(IDictionary<string, string> memberIdsToNames, Action<string, int> onAnalysingTestCase)
        {
            this.memberIdsToNames = memberIdsToNames;
            this.onAnalysingTestCase = onAnalysingTestCase;

            MembersAndCoveringTests = new ReadOnlyDictionary<string, ImmutableHashSet<string>>(membersAndCoveringTests);
            TestEventListener = new NUnitEventListener(OnTestStarting, OnTestComplete, OnTestFixtureComplete);
        }

        public void OnTestEvent(string report)
        {
            TestEventListener.OnTestEvent(report);
        }

        private void OnTestFixtureComplete(IEnumerable<string> testMethodsInFixture, IEnumerable<string> executedMemberIds)
        {
            var testMethodsInFixtureAsArray = testMethodsInFixture.ToArray();

            foreach (var executedMemberId in executedMemberIds)
            {
                var executedMemberName = memberIdsToNames[executedMemberId];

                if (!membersAndCoveringTests.ContainsKey(executedMemberName))
                    membersAndCoveringTests.Add(executedMemberName, ImmutableHashSet<string>.Empty);

                foreach (var testMethodName in testMethodsInFixtureAsArray)
                {
                    membersAndCoveringTests[executedMemberName] =
                        membersAndCoveringTests[executedMemberName].Add(testMethodName);
                }
            }
        }

        private void OnTestStarting(string testMethodName)
        {
            onAnalysingTestCase(testMethodName, numTestCasesExecuted);
            numTestCasesExecuted++;
        }

        private void OnTestComplete(string testMethodName, IEnumerable<string> executedMemberIds)
        {
            foreach (var executedMemberId in executedMemberIds)
            {
                var executedMemberName = memberIdsToNames[executedMemberId];

                if (!membersAndCoveringTests.ContainsKey(executedMemberName))
                    membersAndCoveringTests.Add(executedMemberName, ImmutableHashSet<string>.Empty);

                membersAndCoveringTests[executedMemberName] =
                    membersAndCoveringTests[executedMemberName].Add(testMethodName);
            }
        }
    }
}