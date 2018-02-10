using System;
using System.Xml;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class NUnitTestResults_Tests
    {
         [Test]
        public void When_file_indicates_that_all_tests_pass_Then_returns_AllTestsPass()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" testcasecount=""7"" result=""Passed"">
                  </test-run>
                ");

            var result = NUnitTestResults.Parse(xmlNode);

            Assert.That(result, Is.EqualTo(TestRunnerResult.AllTestsPassed));
        }

        [Test]
        public void When_file_indicates_that_some_tests_failed_Then_returns_SomeTestsFailed()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" testcasecount=""7"" result=""Failed"">
                  </test-run>
                ");

            var result = NUnitTestResults.Parse(xmlNode);

            Assert.That(result, Is.EqualTo(TestRunnerResult.SomeTestsFailed));
        }

        [Test]
        public void When_file_indicates_NUnit_wasnt_able_to_run_tests_Then_throws_an_exception()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" testcasecount=""0"" result=""Failed"">
                  </test-run>
                ");

            Assert.Throws<InvalidOperationException>(() => NUnitTestResults.Parse(xmlNode));
        }

        [Test]
        public void When_file_indicates_NUnit_itself_encountered_an_unexpected_error_Then_throws_an_exception()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" testcasecount=""1"" result=""Failed"">
                     <test-suite runstate=""Runnable"">                        
                     </test-suite>
                     <test-suite runstate=""NotRunnable"">                        
                     </test-suite>
                  </test-run>
                ");

            Assert.Throws<InvalidOperationException>(() => NUnitTestResults.Parse(xmlNode));            
        }

        [TestCase("Inconclusive")]
        [TestCase("Skipped")]
        public void When_file_indicates_NUnit_tests_were_not_run_Then_throws_an_exception(string result)
        {
            var xmlNode = StringToXmlNode(
                $@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                   <test-run id=""2"" testcasecount=""6"" result=""{result}"">
                   </test-run>
                ");

            Assert.Throws<InvalidOperationException>(() => NUnitTestResults.Parse(xmlNode));
        }

        private static XmlNode StringToXmlNode(string text)
        {
            var doc = new XmlDocument();
            doc.LoadXml(text);
            return doc.DocumentElement;
        }
    }
}
