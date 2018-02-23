using System.Xml;
using Fettle.Core.Internal.NUnit;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class NUnitExploreResults_Tests
    {
        [Test]
        public void All_runnable_tests_mentioned_in_Xml_are_parsed()
        {
            var xmlNode = StringToXmlNode(
@"
<test-suite>  
  <test-suite>
    <test-suite>
      <test-suite>
        <test-case fullname=""HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred"" runstate=""Runnable"" />
      </test-suite>
      <test-suite>
        <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothZero"" runstate=""Runnable"" />
        <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothOne"" runstate=""NotRunnable"" />
      </test-suite>
    </test-suite>
  </test-suite>
</test-suite>
");

            var testsFound = NUnitExploreResults.Parse(xmlNode);

            Assert.That(testsFound, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred",
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothZero"
            }));
        }
        
        private static XmlNode StringToXmlNode(string text)
        {
            var doc = new XmlDocument();
            doc.LoadXml(text);
            return doc.DocumentElement;
        }
    }
}
