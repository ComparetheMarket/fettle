using Fettle.Core.Internal.Mutators;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    [TestFixture]
    class If_statement_condition
    {
        [TestCase("a", "if (!(a))")]
        [TestCase("!a", "if (a)")]
        public void Conditions_can_be_negated(string before, string expectedAfter)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
$@"public static string BooleanToString(bool b)
{{
    if ({before})
    {{
        return ""true"";
    }}
    else
    {{
        return ""false"";
    }}
}}");
            var nodeToMutate = syntaxTree.GetRoot().DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Single();

            var mutators = nodeToMutate.SupportedMutators().OfType<InvertIfStatement>().ToArray();
            var mutatedNode = mutators.Select(m => m.Mutate(nodeToMutate)).Single();
            Assert.That(mutatedNode.ToString(), Does.StartWith(expectedAfter));
        }

        [Test]
        public void Conditions_can_be_negated_within_elseif()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
@"public static string BooleansToString(bool a, bool b)
{
    if (a)
    {
        return ""a"";
    }
    else if (b)
    {
        return ""b"";
    }

    return "";
}");
            var nodeToMutate = syntaxTree.GetRoot().DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Skip(1)
                .Single();

            var mutators = nodeToMutate.SupportedMutators().OfType<InvertIfStatement>().ToArray();
            var mutatedNode = mutators.Select(m => m.Mutate(nodeToMutate)).Single();
            Assert.That(mutatedNode.ToString(), Does.StartWith("if (!(b)"));
        }

        [Test]
        public void Conditions_that_have_multiple_parts_have_parentheses_added()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
@"public static string BooleansToString(bool a, bool b)
{
    if (a && b)
    {
        return ""both are true"";
    }
    return "";
}");
            var nodeToMutate = syntaxTree.GetRoot().DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Single();

            var mutators = nodeToMutate.SupportedMutators().OfType<InvertIfStatement>().ToArray();
            var mutatedNode = mutators.Select(m => m.Mutate(nodeToMutate)).Single();
            Assert.That(mutatedNode.ToString(), Does.StartWith("if (!(a && b)"));
        }
    }
}
