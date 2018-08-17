using System.Linq;
using Fettle.Core.Internal;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class CanInstrument_Tests
    {
        [Test]
        public void Normal_methods_can_be_instrumented()
        {
            var methodDeclaration = ExtractLastMethodDeclarationFromSource(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        public static void MagicNumber(int a)
        {
            return 40 + 2;
        }
    }
}");

            Assert.That(methodDeclaration.CanInstrument(), Is.True);
        }

        [Test]
        public void Expression_bodied_methods_can_be_instrumented()
        {
            var methodDeclaration = ExtractLastMethodDeclarationFromSource(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        public static void MagicNumber(int a) => 40 + 2;
    }
}");

            Assert.That(methodDeclaration.CanInstrument(), Is.True);
        }

        [Test]
        public void Abstract_method_declarations_cannot_be_instrumented()
        {
            var methodDeclaration = ExtractLastMethodDeclarationFromSource(@"
namespace DummyNamespace
{
    public abstract class DummyClass
    {
        public abstract void MagicNumber(int a);
    }
}");

            Assert.That(methodDeclaration.CanInstrument(), Is.False);
        }

        [Test]
        public void Methods_that_override_abstract_methods_can_be_instrumented()
        {
            var methodDeclaration = ExtractLastMethodDeclarationFromSource(@"
namespace DummyNamespace
{
    public abstract class DummyAbstractClass
    {
        public abstract void MagicNumber(int a);
    }

    public class DummyClass : DummyAbstractClass
    {
        public override void MagicNumber(int a)
        {
            return 40 + 2;
        }
    }
}");

            Assert.That(methodDeclaration.CanInstrument(), Is.True);
        }

        [Test]
        public void Empty_partial_method_declarations_cannot_be_instrumented()
        {
            var methodDeclaration = ExtractLastMethodDeclarationFromSource(@"
namespace DummyNamespace
{
    public partial class DummyClass
    {
        public partial void MagicNumber(int a);
    }
}");

            Assert.That(methodDeclaration.CanInstrument(), Is.False);
        }

        [Test]
        public void Partial_methods_can_be_instrumented()
        {
            var methodDeclaration = ExtractLastMethodDeclarationFromSource(@"
namespace DummyNamespace
{
    public partial class DummyClass
    {
        public partial void MagicNumber(int a);
    }

    public partial class DummyClass
    {
        public partial void MagicNumber(int a)
        {
            return 40 + 2;
        }
    }
}");

            Assert.That(methodDeclaration.CanInstrument(), Is.True);
        }

        private static MethodDeclarationSyntax ExtractLastMethodDeclarationFromSource(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            return syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Last();
        }
    }
}
