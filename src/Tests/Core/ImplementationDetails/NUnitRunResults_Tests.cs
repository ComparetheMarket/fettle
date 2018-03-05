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
        public void When_results_xml_contains_output_Then_all_ouput_is_collated()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" result=""Failed"" total=""7"">
                     <test-suite runstate=""Runnable"">
                       <test-case>
                         <output>
                           <![CDATA[hello world
hello world again
]]>
                         </output>
                       </test-case>
                     </test-suite>
                     <test-suite runstate=""Runnable"">
                       <test-case>
                         <output>
                           <![CDATA[wibble
womble
]]>
                         </output>
                       </test-case>
                     </test-suite>
                  </test-run>
                ");

            var result = NUnitRunResults.Parse(xmlNode);

            Assert.That(result.ConsoleOutput, Is.EqualTo(
@"hello world
hello world again
wibble
womble
"));
        }

        [Test]
        public void When_results_xml_does_not_contain_output_Then_consoleOutput_is_empty()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" result=""Failed"" total=""7"">
                  </test-run>
                ");

            var result = NUnitRunResults.Parse(xmlNode);

            Assert.That(result.ConsoleOutput, Is.EqualTo(""));
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
