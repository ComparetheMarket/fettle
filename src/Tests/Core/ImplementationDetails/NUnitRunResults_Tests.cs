using System;
using System.Xml;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class NUnitRunResults_Tests
    {
        [Test]
        public void When_results_xml_indicates_that_all_tests_passed_Then_status_is_AllTestsPass()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" result=""Passed"" total=""7"">
                  </test-run>
                ");

            var result = NUnitRunResults.Parse(xmlNode);

            Assert.That(result.Status, Is.EqualTo(TestRunStatus.AllTestsPassed));
        }

        [Test]
        public void When_results_xml_indicates_that_some_tests_failed_Then_status_is_SomeTestsFailed()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" result=""Failed"" total=""7"">
                  </test-run>
                ");

            var result = NUnitRunResults.Parse(xmlNode);

            Assert.That(result.Status, Is.EqualTo(TestRunStatus.SomeTestsFailed));
        }

        [Test]
        public void When_results_xml_indicates_that_some_tests_failed_Then_error_field_contains_error_info()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" result=""Failed"" total=""2"">
                     <test-suite runstate=""Runnable"">
                        <test-suite runstate=""Runnable"">
                            <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsPositive"">
                                <failure>
                                    <message>error message 1</message>
                                    <stack-trace>
                                        <![CDATA[   at HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsPositive() in C:\dev\fettle\src\Examples\HasSurvivingMutants\Tests\PartialNumberComparisonTests.cs:line 14 ]]>
                                    </stack-trace>
                                </failure>
                            </test-case>
                        </test-suite>
                        <test-suite runstate=""Runnable"">
                            <test-case fullname=""HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsNegative"">
                                <failure>
                                    <message>error message 2</message>
                                    <stack-trace>
                                        <![CDATA[   at HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsNegative() in C:\dev\fettle\src\Examples\HasSurvivingMutants\Tests\PartialNumberComparisonTests.cs:line 15 ]]>
                                    </stack-trace>
                                </failure>
                            </test-case>
                        </test-suite>
                     </test-suite>
                  </test-run>
                ");

            var result = NUnitRunResults.Parse(xmlNode);

            Assert.Multiple(() =>
            {
                Assert.That(result.Error, Does.Contain("HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsPositive"));
                Assert.That(result.Error, Does.Contain("PartialNumberComparisonTests.cs:line 14"));
                Assert.That(result.Error, Does.Contain("error message 1"));

                Assert.That(result.Error, Does.Contain("HasSurvivingMutants.Tests.PartialNumberComparisonTests.IsNegative"));
                Assert.That(result.Error, Does.Contain("PartialNumberComparisonTests.cs:line 15"));
                Assert.That(result.Error, Does.Contain("error message 2"));
            });
        }
        
        [Test]
        public void When_results_xml_indicates_no_tests_were_run_Then_throws_an_exception()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" result=""Passed"" total=""0"">
                  </test-run>
                ");

            Assert.Throws<InvalidOperationException>(() => NUnitRunResults.Parse(xmlNode));
        }

        [Test]
        public void When_results_xml_indicates_NUnit_itself_encountered_an_unexpected_error_Then_throws_an_exception()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" result=""Failed"" total=""7"">
                     <test-suite runstate=""Runnable"">
                     </test-suite>
                     <test-suite runstate=""NotRunnable"">
                     </test-suite>
                  </test-run>
                ");

            Assert.Throws<InvalidOperationException>(() => NUnitRunResults.Parse(xmlNode));
        }

        [TestCase("Inconclusive")]
        [TestCase("Skipped")]
        public void When_results_xml_indicates_NUnit_tests_were_not_run_Then_throws_an_exception(string result)
        {
            var xmlNode = StringToXmlNode(
                $@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                   <test-run id=""2"" total=""6"" result=""{result}"">
                   </test-run>
                ");

            Assert.Throws<InvalidOperationException>(() => NUnitRunResults.Parse(xmlNode));
        }

        private static XmlNode StringToXmlNode(string text)
        {
            var doc = new XmlDocument();
            doc.LoadXml(text);
            return doc.DocumentElement;
        }
    }
}
