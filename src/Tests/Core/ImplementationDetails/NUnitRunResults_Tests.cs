using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Xml;
using Fettle.Core;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class NUnitRunResults_Tests
    {
        [Test]
        public void blurgh()
        {
            using (var stream = File.Create(@"C:\temp\fettle_methods_2.bin"))
            {
                var buffer = new byte[1024 * 1024];
                stream.Write(buffer, 0, buffer.Length);   
            }
            using (var memoryMappedFile =
                MemoryMappedFile.CreateFromFile(@"c:\temp\fettle_methods_2.bin", FileMode.Open, @"fettle_methods"))
            {
                Process.Start(@"c:\dev\fettle\src\Console\bin\Debug\fettle.console.exe").WaitForExit();

                //using (var stream = File.OpenRead(filePath))
                using (var file = MemoryMappedFile.OpenExisting(@"fettle_methods"))
                using (var accessor = file.CreateViewAccessor())
                {
                    long loc = 0;
                    while (loc < accessor.Capacity)
                    {
                        var value = accessor.ReadByte(loc);
                        if (value == 0xFF)
                        {
                            System.Console.WriteLine($"method {loc}: CALLED");
                        }
                        else
                        {
                            //System.Console.WriteLine($"method {loc}: not called");
                        }

                        //loc += sizeof(byte) * 4;
                        loc++;
                    }
                }
            }
        }

        [Test]
        public void blah()
        {
//            using (var mmf =
//                MemoryMappedFile.CreateNew("fettle_methods", 1024, MemoryMappedFileAccess.ReadWrite))
            using (var memoryMappedFile = MemoryMappedFile.CreateNew("fettle_coverage", 1024*1024))
            {
                //using (var stream = File.Create(@"C:\temp\fettle_methods"))
                //{
                //    var buffer = new byte[1024 * 1024];
                //    stream.Write(buffer, 0, buffer.Length);   
                //}

                CoverageAnalyser.Collector.MethodCalled(10, "10");
                CoverageAnalyser.Collector.MethodCalled(1, "1");
                CoverageAnalyser.Collector.MethodCalled(3, "3");
                CoverageAnalyser.Collector.MethodCalled(3, "3");
                CoverageAnalyser.Collector.MethodCalled(3, "3");
                CoverageAnalyser.Collector.MethodCalled(1, "1");
                CoverageAnalyser.Collector.MethodCalled(2, "2");

                using (var file = MemoryMappedFile.OpenExisting("fettle_coverage"))
                {
                    using (var accessor = file.CreateViewAccessor())
                    {
                        long loc = 0;
                        for (int i = 0; i < 15; ++i)
                        {
                            var thing = accessor.ReadByte(loc);
                            loc++;
                            System.Console.WriteLine($"{i} => {thing:X}");
                        }

                    }
                }
            }
        }

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
        public void When_results_xml_indicates_that_some_tests_failed_Then_error_reflects_their_errors()
        {
            var xmlNode = StringToXmlNode(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" result=""Failed"" total=""2"">
                     <test-suite runstate=""Runnable"">
                        <test-suite runstate=""Runnable"">
                            <test-case>
                                <failure>
                                    <message>error message 1</message>
                                </failure>
                            </test-case>
                        </test-suite>
                        <test-suite runstate=""Runnable"">
                            <test-case>
                                <failure>
                                    <message>error message 2</message>
                                </failure>
                            </test-case>
                        </test-suite>
                     </test-suite>
                  </test-run>
                ");

            var result = NUnitRunResults.Parse(xmlNode);

            Assert.That(result.Error, Does.Contain("error message 1"));
            Assert.That(result.Error, Does.Contain("error message 2"));
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
