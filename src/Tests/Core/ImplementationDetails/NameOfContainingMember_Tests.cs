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
        public void Members_within_structs_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
            @"namespace DummyNamespace
            {
                public struct DummyStruct
                {
                    public int a, b;

                    public DummyStruct(int x, int y)
                    {
                        a = x;
                        b = y;
                    }
                }
            }");
            var compilation = CSharpCompilation.Create("DummyAssembly", new[] { syntaxTree });
            var returnStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<ExpressionStatementSyntax>().Last();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = returnStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("DummyNamespace.DummyStruct::DummyStruct(System.Int32,System.Int32)"));
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

        [Test]
        public void Constructors_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
            @"namespace DummyNamespace
            {
                public class DummyClass
                {
                    public DummyClass()
                    {
                        System.Console.WriteLine(""hi"");
                    }
                }
            }");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var expressionStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<ExpressionStatementSyntax>().Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = expressionStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("DummyNamespace.DummyClass::DummyClass()"));
        }

        [Test]
        public void Destructors_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
            @"namespace DummyNamespace
            {
                public class DummyClass
                {
                    ~DummyClass()
                    {
                        System.Console.WriteLine(""bye"");
                    }
                }
            }");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var expressionStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<ExpressionStatementSyntax>().Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = expressionStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("DummyNamespace.DummyClass::~DummyClass()"));
        }

        [Test]
        public void Indexers_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"namespace DummyNamespace
            {
                public class DummyClass
                {
                    private int[] arr = new int[100];

                    public int this[int i]
                    {
                        get { return arr[i]; }
                        set { arr[i] = value; }
                    }
                }
            }");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var returnStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = returnStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("System.Int32 DummyNamespace.DummyClass::this[System.Int32]"));
        }

        [Test]
        public void Events_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
            @"namespace DummyNamespace
            {
                using System;

                public class DummyClass
                {
                    private event EventHandler<EventArgs> someEvent;

                    public event EventHandler<EventArgs> SomeEvent
                    {
                        add { someEvent += value; }
                        remove { someEvent -= value; }
                    }
                }
            }");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var addStatmentNode = syntaxTree.GetRoot().DescendantNodes().OfType<AssignmentExpressionSyntax>().First();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = addStatmentNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.EqualTo("EventHandler<EventArgs> DummyNamespace.DummyClass::SomeEvent"));
        }

        [Test]
        public void Operators_are_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
            @"namespace DummyNamespace
            {
                public class DummyClass
                {
                    public double x, y;

                    public static DummyClass operator *(DummyClass a, DummyClass b)
                    {
                        return new DummyClass { x = a.x + b.x, y = a.y + b.y };
                    }

                    public static implicit operator string(DummyClass d)
                    {
                        return d.x.ToString();
                    }
                }
            }");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var returnStatementNodes = syntaxTree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().ToArray();
            
            Assert.Multiple(() =>
            {
                Assert.That(returnStatementNodes[0].NameOfContainingMember(semanticModel), 
                    Is.EqualTo("DummyNamespace.DummyClass::operator *(DummyNamespace.DummyClass,DummyNamespace.DummyClass)"));

                Assert.That(returnStatementNodes[1].NameOfContainingMember(semanticModel), 
                    Is.EqualTo("DummyNamespace.DummyClass::operator string(DummyNamespace.DummyClass)"));
            });
        }

        [Test]
        public void Fields_are_not_supported()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
            @"namespace DummyNamespace
            {
                public static class DummyClass
                {
                    public static int dummyField = 42;
                }
            }");
            var compilation = CSharpCompilation.Create("DummyAssembly", new [] { syntaxTree });
            var returnStatementNode = syntaxTree.GetRoot().DescendantNodes().OfType<EqualsValueClauseSyntax>().Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var containingMemberName = returnStatementNode.NameOfContainingMember(semanticModel);

            Assert.That(containingMemberName, Is.Null);
        }
    }
}
