using System.Linq;
using System.Xml;

namespace HasSurvivingMutants_452.Implementation
{
    public static class PartiallyTestedXmlDocumentComparison
    {
        public static bool HasRootXmlElement(XmlDocument doc)
        {
            return doc.DocumentElement != null;
        }

        public static bool HasBetweenOneAndTwoChildNodes(XmlDocument doc)
        {
            var hasAtLeastOneNode = doc.ChildNodes.Count > 0;
            var hasAtMostTwoNodes = doc.ChildNodes.Count < 3;
            return hasAtLeastOneNode && hasAtMostTwoNodes;
        }

        public static bool HasExactlyOneXmlTextNode(XmlDocument doc)
        {
            return doc.ChildNodes.OfType<XmlNode>().SelectMany(x => x.ChildNodes.OfType<XmlNode>()).SingleOrDefault(x => x.NodeType == XmlNodeType.Text) != null;
        }
    }
}
