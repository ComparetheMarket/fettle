using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Core
{
    class Method_coverage : Contexts.Default
    {
        public Method_coverage()
        {
            Given_an_app_to_be_mutation_tested();

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_only_tests_that_cover_methods_are_run()
        {
            MockTestRunner.Verify(r => r.RunTests(
                It.IsAny<IEnumerable<string>>(),
                It.Is<IEnumerable<string>>(tn =>
                    tn.Contains("example.test.one"))));

            MockTestRunner.Verify(r => r.RunTests(
                It.IsAny<IEnumerable<string>>(),
                It.Is<IEnumerable<string>>(tn =>
                    !tn.Contains("example.test.one"))), Times.Never);
        }
    }
}
