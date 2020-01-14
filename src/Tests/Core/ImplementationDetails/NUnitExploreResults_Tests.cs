using System.Xml;
using Fettle.Core.Internal.NUnit;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class NUnitExploreResults_Tests
    {
        [Test]
        public void Runnable_tests_are_parsed()
        {
            var xmlNode = StringToXmlNode(@"
<test-run>
    <test-suite runstate=""Runnable"">
        <test-suite runstate=""Runnable"">
            <test-case fullname=""HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred"" runstate=""Runnable"" />
        </test-suite>
        <test-suite runstate=""Runnable"">
            <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothZero"" runstate=""Runnable"" />
            <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothOne"" runstate=""NotRunnable"" />
        </test-suite>
    </test-suite>
</test-run>
");

            var testsFound = NUnitExploreResults.Parse(xmlNode);

            Assert.That(testsFound, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred",
                "HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothZero"
            }));
        }

        [TestCase("NotRunnable")]
        [TestCase("Ignored")]
        [TestCase("Explicit")]
        [TestCase("Skipped")]
        public void Non_runnable_tests_are_not_parsed(string testCaseRunState)
        {
            var xmlNode = StringToXmlNode($@"
<test-run>
    <test-suite runstate=""Runnable"">
        <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothFour"" runstate=""{testCaseRunState}"" />
    </test-suite>
</test-run>
");

            var testsFound = NUnitExploreResults.Parse(xmlNode);

            Assert.That(testsFound, Has.Length.Zero);
        }

        [TestCase("NotRunnable")]
        [TestCase("Ignored")]
        [TestCase("Explicit")]
        [TestCase("Skipped")]
        public void Runnable_tests_with_non_runnable_parent_fixtures_are_not_parsed(string parentFixtureRunState)
        {
            var xmlNode = StringToXmlNode($@"
<test-run>
    <test-suite runstate=""Runnable"">
        <test-suite runstate=""{parentFixtureRunState}"">
            <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothZero"" runstate=""Runnable"" />
        </test-suite>
    </test-suite>
</test-run>
");

            var testsFound = NUnitExploreResults.Parse(xmlNode);

            Assert.That(testsFound, Has.Length.Zero);
        }

        [TestCase("NotRunnable")]
        [TestCase("Ignored")]
        [TestCase("Explicit")]
        [TestCase("Skipped")]
        public void Runnable_tests_with_non_runnable_ancestor_fixtures_are_not_parsed(string parentFixtureRunState)
        {
            var xmlNode = StringToXmlNode($@"
<test-run>
    <test-suite runstate=""{parentFixtureRunState}"">
        <test-suite runstate=""Runnable"">
            <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.AreBothZero"" runstate=""Runnable"" />
        </test-suite>
    </test-suite>
</test-run>
");

            var testsFound = NUnitExploreResults.Parse(xmlNode);

            Assert.That(testsFound, Has.Length.Zero);
        }
        
        [Test]
        public void Parameterized_tests_are_parsed()
        {
            var xmlNode = StringToXmlNode(@"
<test-run>
    <test-suite runstate=""Runnable"">
        <test-suite runstate=""Runnable"" type=""ParameterizedMethod"" fullname=""HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred"">
            <test-case fullname=""HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred(5)"" runstate=""Runnable"" />
            <test-case fullname=""HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred(10)"" runstate=""Runnable"" />
            <test-case fullname=""HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred(-1)"" runstate=""Runnable"" />
        </test-suite>
    </test-suite>
</test-run>
");

            var testsFound = NUnitExploreResults.Parse(xmlNode);

            Assert.That(testsFound, Is.EquivalentTo(new[]
            {
                "HasSurvivingMutants.Tests.MorePartialNumberComparisonTests.IsGreaterThanOneHundred"
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
