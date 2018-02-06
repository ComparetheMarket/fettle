using System.Linq;
using Fettle.Core.Internal;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    class Increment_and_decrement_operators
    {
        [TestCase("++n", "--n")]
        [TestCase("--n", "++n")]
        [TestCase("n++", "n--")]
        [TestCase("n--", "n++")]
        public void Operators_are_swappable(string operatorFrom, string operatorTo)
        {
            var rootNode = CSharpSyntaxTree.ParseText(
                $@"public class ExampleClass
                {{
                    public void Increment(int n)
                    {{
                        return {operatorFrom};
                    }}
                }}")
                .GetRoot();

            var nodeToMutate = rootNode.DescendantNodes()
                .Single(n => n.ToString() == operatorFrom);

            var mutators = nodeToMutate.SupportedMutators();
            Assert.That(mutators.Count, Is.EqualTo(1));

            var mutatedNode = mutators.Select(m => m.Mutate(nodeToMutate)).Single();
            Assert.That(mutatedNode.ToString(), Is.EqualTo(operatorTo));
        }
    }
}
