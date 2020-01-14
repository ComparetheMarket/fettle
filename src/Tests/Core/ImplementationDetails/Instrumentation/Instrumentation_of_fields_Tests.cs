using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Fettle.Tests.Core.ImplementationDetails.Instrumentation
{
    [TestFixture]
    class Instrumentation_of_fields_Tests : InstrumentationTestsBase
    {
        [Test]
        public async Task Fields_cannot_be_instrumented()
        {
            var input1 = await CreateInput<FieldDeclarationSyntax>(@"
            namespace DummyNamespace
            {
                public static class DummyClass
                {
                    private static int magicNumber;
                }
            }");
            var input2 = await CreateInput<FieldDeclarationSyntax>(@"
            namespace DummyNamespace
            {
                public static class DummyClass
                {
                    private static int magicNumber = 41 + 1;
                }
            }");

            Assert.That(input1.MemberToInstrument.CanInstrument(), Is.False);
            Assert.That(input2.MemberToInstrument.CanInstrument(), Is.False);
        }
    }
}
