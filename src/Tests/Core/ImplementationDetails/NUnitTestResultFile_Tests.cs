using System;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class NUnitTestResultFile_Tests
    {
        [Test]
        public void When_file_indicates_that_all_tests_pass_Then_returns_AllTestsPass()
        {
            var fileContents =
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" testcasecount=""7"" result=""Passed"">
                  </test-run>
                ";

            var result = NUnitTestResultFile.ParseResultFileContents(fileContents);

            Assert.That(result, Is.EqualTo(TestRunnerResult.AllTestsPassed));
        }

        [Test]
        public void When_file_indicates_that_some_tests_failed_Then_returns_SomeTestsFailed()
        {
            var fileContents =
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" testcasecount=""7"" result=""Failed"">
                  </test-run>
                ";

            var result = NUnitTestResultFile.ParseResultFileContents(fileContents);

            Assert.That(result, Is.EqualTo(TestRunnerResult.SomeTestsFailed));
        }

        [Test]
        public void When_file_indicates_NUnit_wasnt_able_to_run_tests_Then_throws_an_exception()
        {
            var fileContents =
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" testcasecount=""0"" result=""Failed"">
                  </test-run>
                ";

            Assert.Throws<InvalidOperationException>(() => NUnitTestResultFile.ParseResultFileContents(fileContents));
        }

        [Test]
        public void When_file_indicates_NUnit_itself_encountered_an_unexpected_error_Then_throws_an_exception()
        {
            var fileContents =
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                  <test-run id=""2"" testcasecount=""1"" result=""Failed"">
                     <test-suite runstate=""Runnable"">                        
                     </test-suite>
                     <test-suite runstate=""NotRunnable"">                        
                     </test-suite>
                  </test-run>
                ";

            Assert.Throws<InvalidOperationException>(() => NUnitTestResultFile.ParseResultFileContents(fileContents));            
        }

        [TestCase("Inconclusive")]
        [TestCase("Skipped")]
        public void When_file_indicates_NUnit_tests_were_not_run_Then_throws_an_exception(string result)
        {
            var fileContents =
                $@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
                   <test-run id=""2"" testcasecount=""6"" result=""{result}"">
                   </test-run>
                ";

            Assert.Throws<InvalidOperationException>(() => NUnitTestResultFile.ParseResultFileContents(fileContents));
        }
    }
}
