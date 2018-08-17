using System.Linq;
using Fettle.Core.Internal;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Fettle.Tests.Core.Contexts
{
    class BinaryExpressionMutators
    {
        protected static void AssertBinaryExpressionMutatesTo(string fromOperator, params string[] toOperators)
        {
            var rootNode = CSharpSyntaxTree.ParseText(
                $@"public class ExampleClass
                {{
                    public bool IsPositive(int n)
                    {{
                        return n {fromOperator} 0;
                    }}
                }}")
                .GetRoot();

            var nodeToMutate = rootNode.DescendantNodes()
                .Single(n => n.ToString() == $"n {fromOperator} 0");

            var mutators = nodeToMutate.SupportedMutators();
            Assert.That(mutators.Count, Is.EqualTo(toOperators.Length));

            var mutatedNodes = mutators.Select(m => m.Mutate(nodeToMutate)).ToList();
            foreach (var toOperator in toOperators)
            {
                Assert.That(mutatedNodes.Any(mn => mn.ToString().Contains($"n {toOperator} 0")),
                    $"Failed to find mutated node that contained expected operator \"{toOperator}\"");
            }
        }
    }
}