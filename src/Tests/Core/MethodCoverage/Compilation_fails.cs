using NUnit.Framework;

namespace Fettle.Tests.Core.MethodCoverage
{
    class Compilation_fails : Contexts.Coverage
    {
        public Compilation_fails()
        {
            Given_an_app_that_does_not_compile();
            Given_project_filters("Implementation");
            
            When_analysing_method_coverage(catchExceptions: true);
        }

        [Test]
        public void Then_an_exception_is_thrown()
        {
            Assert.That(ThrownException, Is.Not.Null);
        }
    }
}