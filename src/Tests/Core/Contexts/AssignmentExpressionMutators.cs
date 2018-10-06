using System.Linq;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Fettle.Tests.Core.Contexts
{
    class AssignmentExpressionMutators
    {
        protected static void AssertAssignmentExpressionMutatesTo(string fromOperator, params string[] toOperators)
        {
            var rootNode = CSharpSyntaxTree.ParseText(
                    $@"public class ExampleClass
                    {{
                        private int n;

                        public void Modify(int d)
                        {{
                            return n {fromOperator} d;
                        }}
                    }}")
                    .GetRoot();

            var nodeToMutate = rootNode.DescendantNodes()
                .Single(n => n.ToString() == $"n {fromOperator} d");

            var mutators = nodeToMutate.SupportedMutators();
            Assert.That(mutators.Count, Is.EqualTo(toOperators.Length));

            var mutatedNodes = mutators.Select(m => m.Mutate(nodeToMutate)).ToList();
            foreach (var toOperator in toOperators)
            {
                Assert.That(mutatedNodes.Any(mn => mn.ToString().Contains($"n {toOperator} d")),
                    $"Failed to find mutated node that contained expected operator \"{toOperator}\"");
            }
        }
    }
}