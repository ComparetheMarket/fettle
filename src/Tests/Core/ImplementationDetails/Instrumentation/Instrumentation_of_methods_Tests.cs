using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Threading.Tasks;
using InstrumentationImpl = Fettle.Core.Internal.Instrumentation;

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
            public async Task Expression_bodied_methods_that_do_not_return_a_valud_can_be_instrumented()
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
    }
}
