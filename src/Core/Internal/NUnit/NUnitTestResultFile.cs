using System;
using System.Xml;

namespace Fettle.Core.Internal.NUnit
{
    internal static class NUnitTestResultFile
    {
        public static TestRunnerResult ParseResultFileContents(string fileContents)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(fileContents);

            var testRun = xmlDocument.SelectSingleNode("test-run");
            var testCaseCount = int.Parse(testRun.Attributes["testcasecount"].Value);
            if (testCaseCount == 0)
            {
                throw new InvalidOperationException("Failed to run any tests");
            }

            foreach (XmlNode testSuite in testRun.SelectNodes("test-suite"))
            {
                var runState = testSuite.Attributes["runstate"].Value;
                if (runState == "NotRunnable")
                {
                    throw new InvalidOperationException("One or more NUnit test suites were not runnable");
                }
            }

            var result = testRun.Attributes["result"].Value;
            switch (result)
            {
                case "Passed": return TestRunnerResult.AllTestsPassed;
                case "Failed": return TestRunnerResult.SomeTestsFailed;
                default: throw new InvalidOperationException($"Unexpected NUnit test run result: \"{result}\"");
            }
        }
    }
}