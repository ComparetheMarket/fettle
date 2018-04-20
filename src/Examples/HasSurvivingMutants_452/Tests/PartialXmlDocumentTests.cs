using System.Xml;
using HasSurvivingMutants_452.Implementation;
using NUnit.Framework;

namespace HasSurvivingMutants_452.Tests
{
    // This fixture will (deliberately) produce surviving mutants because its tests are 
    // not comprehensive.

    public class PartialXmlDocumentTests
    {
        private readonly XmlDocument doc;

        public PartialXmlDocumentTests()
        {
            this.doc = new XmlDocument();
            doc.LoadXml("<?xml version='1.0' encoding='utf-8'?><wibble>Hello</wibble>");
        }

        [Test]
        public void HasRootXmlElement()
        {
            Assert.That(PartiallyTestedXmlDocumentComparison.HasRootXmlElement(doc), Is.True);
        }

        [Test]
        public void HasBetweenOneAndTwoChildNodes()
        {
            // Tests will still pass when the implementation's "&&" is mutated to a "||", ">" to ">=" and "<" to "<="
            Assert.That(PartiallyTestedXmlDocumentComparison.HasBetweenOneAndTwoChildNodes(doc), Is.True);
        }

        [Test]
        public void HasExactlyOneXmlTextNode()
        {
            Assert.That(PartiallyTestedXmlDocumentComparison.HasExactlyOneXmlTextNode(doc), Is.True);
        }
    }
}
