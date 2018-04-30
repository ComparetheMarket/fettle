using System.Linq;
using System.Xml;

namespace Fettle.Core.Internal.NUnit
{
    internal static class NUnitExploreResults
    {
        public static string[] Parse(XmlNode rootNode)
        {
            return rootNode.ChildNodes
                .Cast<XmlNode>()
                .Where(IsRunnableTestSuite)
                .SelectMany(ParseTestSuite)
                .ToArray();
        }

        private static string[] ParseTestSuite(XmlNode node)
        {
            var testsInThisSuite = node.ChildNodes
                .Cast<XmlNode>()
                .Where(IsRunnableTestCase)
                .Select(n => n.Attributes["fullname"].Value);

            var testsInDescendantSuites = node.ChildNodes
                .Cast<XmlNode>()
                .Where(IsRunnableTestSuite)
                .SelectMany(ParseTestSuite);

            return testsInThisSuite
                .Concat(testsInDescendantSuites)
                .ToArray();
        }

        private static bool IsRunnable(XmlNode n) 
            => n.Attributes["runstate"]?.Value == "Runnable";
        
        private static bool IsRunnableTestSuite(XmlNode n)
            => IsRunnable(n) && n.Name == "test-suite";
        
        private static bool IsRunnableTestCase(XmlNode n)
            => IsRunnable(n) && n.Name == "test-case";
    }
}