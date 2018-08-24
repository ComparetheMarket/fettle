using System.Linq;
using Fettle.Core.Internal.RoslynExtensions;
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
            var methodDeclaration = ExtractLastSyntaxNodeFromSource<MethodDeclarationSyntax>(@"
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
        public void Properties_with_backing_fields_can_be_instrumented()
        {
            var propertyDeclaration = ExtractLastSyntaxNodeFromSource<PropertyDeclarationSyntax>(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        private static int magicNumber = 41;

        public static int MagicNumber
        {
            get { return magicNumber + 1; }
            set { magicNumber = value + 1; }
        }
    }
}");

            Assert.That(propertyDeclaration.CanInstrument(), Is.True);
        }

        [Test]
        public void Expression_bodied_methods_can_be_instrumented()
        {
            var methodDeclaration = ExtractLastSyntaxNodeFromSource<MethodDeclarationSyntax>(@"
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
        public void Expression_bodied_properties_can_be_instrumented()
        {
            var propertyDeclaration = ExtractLastSyntaxNodeFromSource<PropertyDeclarationSyntax>(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        private static int magicNumber = 41;

        public static int MagicNumber
        {
            get => magicNumber + 1;
            set => magicNumber = value + 1; }
        }
    }
}");

            Assert.That(propertyDeclaration.CanInstrument(), Is.True);
        }

        [Test]
        public void Properties_where_getter_and_setters_are_different_can_be_instrumented()
        {
            var propertyDeclaration1 = ExtractLastSyntaxNodeFromSource<PropertyDeclarationSyntax>(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        private static int magicNumber = 41;

        public static int MagicNumber
        {
            get { return magicNumber + 1; }
            set => magicNumber = value + 1;
        }
    }
}");
            var propertyDeclaration2 = ExtractLastSyntaxNodeFromSource<PropertyDeclarationSyntax>(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        private static int magicNumber = 41;

        public static int MagicNumber
        {
            get => magicNumber + 1;
            set { magicNumber = value + 1; }
        }
    }
}");

            Assert.That(propertyDeclaration1.CanInstrument(), Is.True);
            Assert.That(propertyDeclaration2.CanInstrument(), Is.True);
        }

        [Test]
        public void Fields_cannot_be_instrumented()
        {
            var fieldDeclaration1 = ExtractLastSyntaxNodeFromSource<FieldDeclarationSyntax>(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        private static int magicNumber;
    }
}");
            var fieldDeclaration2 = ExtractLastSyntaxNodeFromSource<FieldDeclarationSyntax>(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        private static int magicNumber = 41 + 1;
    }
}");

            Assert.That(fieldDeclaration1.CanInstrument(), Is.False);
            Assert.That(fieldDeclaration2.CanInstrument(), Is.False);
        }

        [Test]
        public void Abstract_method_declarations_cannot_be_instrumented()
        {
            var methodDeclaration = ExtractLastSyntaxNodeFromSource<MethodDeclarationSyntax>(@"
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
            var methodDeclaration = ExtractLastSyntaxNodeFromSource<MethodDeclarationSyntax>(@"
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
            var methodDeclaration = ExtractLastSyntaxNodeFromSource<MethodDeclarationSyntax>(@"
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
        public void Auto_properties_cannot_be_instrumented()
        {
            var propertyDeclaration = ExtractLastSyntaxNodeFromSource<PropertyDeclarationSyntax>(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        public static int MagicNumber { get; set; }
    }
}");

            Assert.That(propertyDeclaration.CanInstrument(), Is.False);
        }

        [Test]
        public void Partial_methods_can_be_instrumented()
        {
            var methodDeclaration = ExtractLastSyntaxNodeFromSource<MethodDeclarationSyntax>(@"
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

        private static T ExtractLastSyntaxNodeFromSource<T>(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            return syntaxTree.GetRoot().DescendantNodes().OfType<T>().Last();
        }
    }
}
