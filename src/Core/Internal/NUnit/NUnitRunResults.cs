using System;
using System.Text;
using System.Xml;

namespace Fettle.Core.Internal.NUnit
{
    internal static class NUnitRunResults
    {
        public static TestRunResult Parse(XmlNode rootNode)
        {
            var numTestsRun = int.Parse(rootNode.Attributes["total"].Value);
            if (numTestsRun == 0)
            {
                throw new InvalidOperationException("Failed to run any tests");
            }

            foreach (XmlNode testSuite in rootNode.SelectNodes("test-suite"))
            {
                var runState = testSuite.Attributes["runstate"].Value;
                if (runState == "NotRunnable")
                {
                    throw new InvalidOperationException("One or more NUnit test suites were not runnable");
                }
            }

            var resultValue = rootNode.Attributes["result"].Value;
            TestRunStatus status;
            switch (resultValue)
            {
                case "Passed": status = TestRunStatus.AllTestsPassed; break;
                case "Failed": status = TestRunStatus.SomeTestsFailed; break;
                default: throw new InvalidOperationException($"Unexpected NUnit test run result: \"{resultValue}\"");
            }

            var consoleOutput = new StringBuilder();
            foreach (XmlNode outputNode in rootNode.SelectNodes("//test-case/output"))
            {
                consoleOutput.Append(outputNode.InnerText);
            }

            return new TestRunResult
            {
                Status = status,
                ConsoleOutput = consoleOutput.ToString()
            };
        }
    }
}