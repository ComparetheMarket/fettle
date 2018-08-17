using System.Linq;
using Fettle.Core.Internal;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class ToSourceText_Tests
    {
        [Test]
        public void The_source_text_of_syntax_can_be_extracted()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
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
            var syntaxRoot = syntaxTree.GetRoot();
            var span = syntaxTree.GetRoot().DescendantNodes().OfType<BinaryExpressionSyntax>().Single().Span;

            var sourceText = span.ToSourceText(syntaxRoot);

            Assert.That(sourceText, Is.EqualTo("            return 40 + 2;"));
        }

        [Test]
        public void The_source_text_of_syntax_that_spans_multiple_lines_can_be_extracted()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace DummyNamespace
{
    public static class DummyClass
    {
        public static bool AreBothZero(int a, int b)
        {
            return a == 0
                   &&
                   b == 0;
        }
    }
}");
            var syntaxRoot = syntaxTree.GetRoot();
            var span = syntaxTree.GetRoot().DescendantNodes()
                .OfType<BinaryExpressionSyntax>()
                .Single(s => s.Kind() == SyntaxKind.LogicalAndExpression)
                .Span;

            var sourceText = span.ToSourceText(syntaxRoot);

            Assert.That(sourceText, Is.EqualTo(
@"            return a == 0
                   &&
                   b == 0;"));
        }
    }
}
