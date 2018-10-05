using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Threading.Tasks;
using InstrumentationImpl = Fettle.Core.Internal.Instrumentation.Instrumentation;

namespace Fettle.Tests.Core.ImplementationDetails.Instrumentation
{
    [TestFixture]
    class Instrumentation_of_methods_Tests
    {
        [TestFixture]
        public class Normal_methods : InstrumentationTestsBase
        {
            [Test]
            public async Task Normal_methods_can_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        public static int MagicNumber(int a)
                        {
                            return 40 + 2;
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<MethodDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public static int MagicNumber(int a)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("    return 40 + 2;"));
            }

            [Test]
            public async Task Normal_methods_that_are_empty_can_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        public static void DoNothing()
                        {
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<MethodDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public static void DoNothing()"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("}"));
            }
        }

        [TestFixture]
        public class Expression_bodied_methods : InstrumentationTestsBase
        {
            [Test]
            public async Task Expression_bodied_methods_that_return_a_values_can_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        public static int MagicNumber(int a) => 40 + 2;
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<MethodDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public static int MagicNumber(int a)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("    return 40 + 2;"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }

            [Test]
            public async Task Expression_bodied_methods_that_do_not_return_a_value_can_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        public void DoSomething() => System.Console.WriteLine(""hello"");
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<MethodDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public void DoSomething()"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("    System.Console.WriteLine(\"hello\");"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }

            [Test]
            public async Task Expression_bodied_methods_with_return_type_that_is_not_predefined_type_can_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        public Task<System.Generic.List<int>> GetThings() => new[]{1,2,3}.ToList();        
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<MethodDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public Task<System.Generic.List<int>> GetThings()"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   return new[]{1, 2, 3}.ToList();"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }
        }
        
        [TestFixture]
        public class Abstract_methods : InstrumentationTestsBase
        {
            [Test]
            public async Task Abstract_method_declarations_cannot_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public abstract class DummyClass
                    {
                        public abstract void MagicNumber(int a);
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.False);
            }

            [Test]
            public async Task Methods_that_override_abstract_methods_can_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public abstract class DummyAbstractClass
                    {
                        public abstract void MagicNumber(int a);
                    }

                    public class DummyClass : DummyAbstractClass
                    {
                        public override void MagicNumber(int a)
                        {
                            return 40 + 2;
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<MethodDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public override void MagicNumber(int a)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   return 40 + 2;"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }
        }

        [TestFixture]
        public class Partial_methods : InstrumentationTestsBase
        {
            [Test]
            public async Task Empty_partial_method_declarations_cannot_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public partial class DummyClass
                    {
                        public partial void MagicNumber(int a);
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.False);
            }

            [Test]
            public async Task Partial_methods_can_be_instrumented()
            {
                var input = await CreateInput<MethodDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public partial class DummyClass
                    {
                        public partial void MagicNumber(int a);
                    }

                    public partial class DummyClass
                    {
                        public partial void MagicNumber(int a)
                        {
                            return 40 + 2;
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<MethodDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public partial void MagicNumber(int a)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   return 40 + 2;"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }
        }

        [TestFixture]
        public class Constructors : InstrumentationTestsBase
        {
            [Test]
            public async Task Constructors_can_be_mutated()
            {
                var input = await CreateInput<ConstructorDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class DummyClass
                    {
                        private readonly int thing;

                        public DummyClass(int a)
                        {
                            this.thing = a + 1;
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<ConstructorDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public DummyClass(int a)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   this.thing = a + 1;"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }

            [Test]
            public async Task Constructors_that_are_expression_bodied_can_be_mutated()
            {
                var input = await CreateInput<ConstructorDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class DummyClass
                    {
                        private readonly int thing;

                        public DummyClass(int a) => this.thing = a + 1;
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<ConstructorDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public DummyClass(int a)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   this.thing = a + 1;"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }
        }

        [TestFixture]
        public class Destructors : InstrumentationTestsBase
        {
            [Test]
            public async Task Destructors_can_be_mutated()
            {
                var input = await CreateInput<DestructorDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class DummyClass
                    {
                        ~DummyClass()
                        {
                            System.Console.WriteLine(""bye!"");
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<DestructorDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("~DummyClass()"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   System.Console.WriteLine(\"bye!\");"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }

            [Test]
            public async Task Destructors_that_are_expression_bodied_can_be_mutated()
            {
                var input = await CreateInput<DestructorDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class DummyClass
                    {
                        ~DummyClass() => System.Console.WriteLine(""bye!"");
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<DestructorDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("~DummyClass()"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   System.Console.WriteLine(\"bye!\");"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }
        }

        [TestFixture]
        public class Operators : InstrumentationTestsBase
        {
            [Test]
            public async Task Operators_can_be_mutated()
            {
                var input = await CreateInput<OperatorDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class Point
                    {
                        public int x, y;
    
                        public Point(int x, int y)
                        {
                            this.x = x;
                            this.y = y;
                        }

                        public static Point operator *(Point a, Point b)
                        {
                            return new Point(a.x * b.x, a.y * b.y);
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<OperatorDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public static Point operator *(Point a, Point b)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   return new Point(a.x * b.x, a.y * b.y);"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));                
            }

            [Test]
            public async Task Operators_that_are_expression_bodied_can_be_mutated()
            {
                var input = await CreateInput<OperatorDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class Point
                    {
                        public int x, y;
    
                        public Point(int x, int y)
                        {
                            this.x = x;
                            this.y = y;
                        }

                        public static Point operator *(Point a, Point b) => new Point(a.x * b.x, a.y * b.y);
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});

                var instrumentedMethodSource = SourceOfInstrumentedMember<OperatorDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public static Point operator *(Point a, Point b)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("   return new Point(a.x * b.x, a.y * b.y);"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));                
            }
        }

        [TestFixture]
        public class Conversion_operators : InstrumentationTestsBase
        {
            [Test]
            public async Task Conversion_operators_can_be_mutated()
            {
                var input = await CreateInput<ConversionOperatorDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class Point
                    {
                        public int x, y;
    
                        public Point(int x, int y)
                        {
                            this.x = x;
                            this.y = y;
                        }

                        public static implicit operator string (Point p)
                        {
                            return x.ToString() + "","" + y.ToString();
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});
                
                var instrumentedMethodSource = SourceOfInstrumentedMember<ConversionOperatorDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public static implicit operator string (Point p)"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[3], Does.Contain(@"   return x.ToString() + "","" + y.ToString();"));
                Assert.That(instrumentedMethodSource[4], Does.Contain("}"));
            }

            [Test]
            public async Task Conversion_operators_that_are_expression_bodied_can_be_mutated()
            {
                var input = await CreateInput<ConversionOperatorDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class Point
                    {
                        public int x, y;
    
                        public Point(int x, int y)
                        {
                            this.x = x;
                            this.y = y;
                        }

                        public static implicit operator string (Point p) => x.ToString() + "","" + y.ToString();
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {});
                
                var instrumentedMethodSource2 = SourceOfInstrumentedMember<ConversionOperatorDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource2[0], Does.Contain("public static implicit operator string (Point p)"));
                Assert.That(instrumentedMethodSource2[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource2[2], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource2[3], Does.Contain(@"   return x.ToString() + "","" + y.ToString();"));
                Assert.That(instrumentedMethodSource2[4], Does.Contain("}"));
            }
        }
    }
}
