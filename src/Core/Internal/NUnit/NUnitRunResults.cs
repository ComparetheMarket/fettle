using System;
using System.Xml;

namespace Fettle.Core.Internal.NUnit
{
    internal static class NUnitRunResults
    {
        public static TestRunnerResult Parse(XmlNode rootNode)
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

            var result = rootNode.Attributes["result"].Value;
            switch (result)
            {
                case "Passed": return TestRunnerResult.AllTestsPassed;
                case "Failed": return TestRunnerResult.SomeTestsFailed;
                default: throw new InvalidOperationException($"Unexpected NUnit test run result: \"{result}\"");
            }
        }
    }
}