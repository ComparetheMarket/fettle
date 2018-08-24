using System.Linq;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class NameOfContainingMember_Tests
    {
        [Test]
        public void Parameter_names_are_fully_qualified()
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
        public void Members_that_return_predefined_types_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
@"namespace DummyNamespace
{
    public static class DummyClass
    {
        public static int[] DummyMethod(int a)
        {
            return new []{ 1, 2, 3 };
        }
    }
}");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var returnStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = returnStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("System.Int32[] DummyNamespace.DummyClass::DummyMethod(System.Int32)"));
        }

        [Test]
        public void Properties_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
@"namespace DummyNamespace
{
    public static class DummyClass
    {
        public static int DummyProperty
        {
            get { return 42; }
        }
    }
}");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var returnStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = returnStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("System.Int32 DummyNamespace.DummyClass::DummyProperty"));
        }
    }
}
