using System.Linq;
using Fettle.Core.Internal.Mutators;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    [TestFixture]
    class Null_coalescing_operator
    {
        [Test]
        public void Operands_can_be_swapped()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                "public static string LeftOrRight(string left, string right) => left ?? right;");

            var nodeToMutate = syntaxTree.GetRoot().DescendantNodes()
                .OfType<BinaryExpressionSyntax>()
                .Single(s => s.Kind() == SyntaxKind.CoalesceExpression);

            var mutators = nodeToMutate.SupportedMutators().OfType<InvertNullCoalescing>().ToArray();
            var mutatedNode = mutators.Select(m => m.Mutate(nodeToMutate)).Single();

            Assert.That(mutatedNode.ToString(), Does.StartWith("right ?? left"));
        }
    }
}