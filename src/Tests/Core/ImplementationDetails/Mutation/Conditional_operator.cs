using System.Linq;
using Fettle.Core.Internal;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    class Conditional_operator
    {
        [Test]
        public void Expressions_are_swappable()
        {
            var rootNode = CSharpSyntaxTree.ParseText(
                $@"public class ExampleClass
                {{
                    public bool IsPositive(int n)
                    {{
                        return n > 0 ? true : false;
                    }}
                }}")
                .GetRoot();

            var nodeToMutate = rootNode.DescendantNodes()
                .OfType<ConditionalExpressionSyntax>()
                .Single();

            var mutators = nodeToMutate.SupportedMutators();
            Assert.That(mutators.Count, Is.EqualTo(1));

            var mutatedNode = mutators.Select(m => m.Mutate(nodeToMutate)).Single();
            Assert.That(mutatedNode.ToString(), Is.EqualTo("n > 0 ? false : true"));
        }
    }
}