﻿using Fettle.Core;
using Fettle.Core.Internal;
using NUnit.Framework;
using System.IO;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class CustomTestRunner_Tests
    {
        [Test]
        public void When_the_command_returns_zero_then_assume_all_tests_have_passed()
        {
            var testRunner = new CustomTestRunner(CustomTestRunnerFilePath("command-returning-zero.bat"));
            
            var result = testRunner.RunAllTests(new[] { "a.dll" });
            
            Assert.That(result.Status, Is.EqualTo(TestRunStatus.AllTestsPassed));
        }

        [Test]
        public void Test_assemblies_are_passed_to_command_as_extra_arguments()
        {
            var testRunner = new CustomTestRunner(CustomTestRunnerFilePath("command-returning-zero-when-correct-arguments-given.bat a"));

            var result = testRunner.RunAllTests(new[] { "b", "c" });

            Assert.That(result.Status, Is.EqualTo(TestRunStatus.AllTestsPassed));
        }

        [Test]
        public void When_the_command_returns_one_then_assume_some_tests_failed()
        {
            var testRunner = new CustomTestRunner(CustomTestRunnerFilePath("command-returning-one.bat"));

            var result = testRunner.RunAllTests(new[] { "a.dll" });

            Assert.That(result.Status, Is.EqualTo(TestRunStatus.SomeTestsFailed));
        }

        [Test]
        public void When_the_command_returns_an_unexpected_value_then_assume_an_unexpected_error_occurred()
        {
            var testRunner = new CustomTestRunner(CustomTestRunnerFilePath("command-returning-three.bat"));

            Assert.Catch(() => testRunner.RunAllTests(new[] { "a.dll" }));
        }

        [Test]
        public void Running_specific_tests_is_not_supported()
        {
            var testRunner = new CustomTestRunner(CustomTestRunnerFilePath("command-returning-zero.bat"));
            
            Assert.Catch(() => testRunner.RunTests(new []{ "a.dll" }, new [] { "testOne" }));
        }

        private static string CustomTestRunnerFilePath(string filename) =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Core", "ImplementationDetails", "TestData", filename);
    }
}
