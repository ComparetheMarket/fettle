using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Engine;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitEventListener : ITestEventListener
    {
        private readonly Action<string> onTestStarting;
        private readonly Action<string, IEnumerable<string>> onTestComplete;
        private readonly Action<IEnumerable<string>, IEnumerable<string>> onTestFixtureComplete;

        private readonly List<string> testCasesWithinFixture = new List<string>();

        public NUnitEventListener(
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
            if (report.StartsWith("<test-suite"))
            {
                HandleTestFixtureComplete(report);
            }
            else if (report.StartsWith("<start-test"))
            {
                HandleTestStarting(report);
            }
            else if (report.StartsWith("<test-case"))
            {
                HandleTestComplete(report);
            }
        }
        
        private void HandleTestStarting(string report)
        {
            var doc = XDocument.Parse(report);

            var testName = doc.Root.Attribute("fullname").Value;

            onTestStarting(testName);
        }

        private void HandleTestComplete(string report)
        {
            var doc = XDocument.Parse(report);

            var testName = doc.Root.Attribute("fullname").Value;
            testCasesWithinFixture.Add(testName);

            var calledMemberIds = ParseExecutedMemberIdsFromOutput(doc);

            onTestComplete(testName, calledMemberIds);
        }

        private void HandleTestFixtureComplete(string report)
        {
            var doc = XDocument.Parse(report);

            var calledMemberIds = ParseExecutedMemberIdsFromOutput(doc);
            onTestFixtureComplete(testCasesWithinFixture, calledMemberIds);

            testCasesWithinFixture.Clear();
        }

        private static List<string> ParseExecutedMemberIdsFromOutput(XDocument doc)
        {
            var consoleOutput = new StringBuilder();
            foreach (var outputNode in doc.XPathSelectElements("//test-case/output|//test-suite/output"))
            {
                consoleOutput.Append(outputNode.Value);
            }

            var calledMemberIds = new List<string>();
            foreach (var outputLine in consoleOutput
                .ToString()
                .Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                const string prefix = Instrumentation.CoverageOutputLinePrefix;
                if (outputLine.StartsWith(prefix))
                {
                    var member = outputLine.Substring(prefix.Length);
                    calledMemberIds.Add(member);
                }
            }

            return calledMemberIds;
        }
    }
}