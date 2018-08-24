using System.Linq;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class NameOfContainingMethod_Tests
    {
        [Test]
        public void Types_of_parameters_within_result_are_fully_qualified()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
@"namespace DummyNamespace
{
    public static class DummyClass
    {
        public static void MyDummyMethod(int a)
        {
            return 42;
        }
    }
}");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var returnStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = returnStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("System.Void DummyNamespace.DummyClass::MyDummyMethod(System.Int32)"));
        }

        [Test]
        public void Types_with_no_namespaces_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
@"namespace DummyNamespace
{
    public static class DummyClass
    {
        public static int[] MyDummyMethod(int a)
        {
            return new []{ 1, 2, 3 };
        }
    }
}");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var returnStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = returnStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("System.Int32[] DummyNamespace.DummyClass::MyDummyMethod(System.Int32)"));
        }
    }
}
