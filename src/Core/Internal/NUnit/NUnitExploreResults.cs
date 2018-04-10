using System.Collections.Generic;
using System.Xml;

namespace Fettle.Core.Internal.NUnit
{
    internal static class NUnitExploreResults
    {
        public static string[] Parse(XmlNode rootNode)
        {
            var testCaseNames = new List<string>();

            foreach (XmlNode testCase in rootNode.SelectNodes("//test-case"))
            {
                var runState = testCase.Attributes["runstate"].Value;
                if (runState.ToLowerInvariant() == "runnable")
                {
                    var fullTestName = testCase.Attributes["fullname"].Value;
                    testCaseNames.Add(fullTestName);
                }
            }

            return testCaseNames.ToArray();
        }
    }
}