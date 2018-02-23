using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Finding_tests : Contexts.Default
    {
        public Finding_tests()
        {
            Given_an_app_to_be_mutation_tested();
            Given_tests_will_be_found(new[]{ "example.test.one", "example.test.two" });

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_each_test_that_is_found_is_run()
        {
            MockTestRunner.Verify(r => r.RunTests(
                It.IsAny<IEnumerable<string>>(),
                It.Is<IEnumerable<string>>(tn =>
                    tn.Contains("example.test.one") && tn.Contains("example.test.two"))));
        }
    }
}
