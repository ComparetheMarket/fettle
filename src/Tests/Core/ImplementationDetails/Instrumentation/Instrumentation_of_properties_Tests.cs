using System;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Threading.Tasks;
using InstrumentationImpl = Fettle.Core.Internal.Instrumentation.Instrumentation;

namespace Fettle.Tests.Core.ImplementationDetails.Instrumentation
{
    [TestFixture]
    class Instrumentation_of_properties_Tests : InstrumentationTestsBase
    {
        [TestFixture]
        public class Normal_properties : InstrumentationTestsBase
        {
            [Test]
            public async Task Normal_properties_with_backing_fields_can_be_instrumented()
            {
                var input = await CreateInput<PropertyDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        private int magicNumber = 41;

                        public int MagicNumber
                        {
                            get { return magicNumber + 1; }
                            set { magicNumber = value + 1; }
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                        input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

                var instrumentedMethodSource = SourceOfInstrumentedMember<PropertyDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[ 0], Does.Contain("public int MagicNumber"));
                Assert.That(instrumentedMethodSource[ 1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 2], Does.Contain("get"));
                Assert.That(instrumentedMethodSource[ 3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[ 5], Does.Contain("    return magicNumber + 1;"));
                Assert.That(instrumentedMethodSource[ 6], Does.Contain("}"));
                Assert.That(instrumentedMethodSource[ 8], Does.Contain("set"));
                Assert.That(instrumentedMethodSource[ 9], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[10], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[11], Does.Contain("    magicNumber = value + 1;"));
                Assert.That(instrumentedMethodSource[12], Does.Contain("}"));
            }

            [Test]
            public async Task Normal_properties_with_only_a_setter_can_be_instrumented()
            {
                var input = await CreateInput<PropertyDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        private int magicNumber = 41;

                        public int MagicNumber
                        {            
                            set { magicNumber = value + 1; }
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                        input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

                var instrumentedMethodSource = SourceOfInstrumentedMember<PropertyDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public int MagicNumber"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain("set"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[5], Does.Contain("    magicNumber = value + 1;"));
                Assert.That(instrumentedMethodSource[6], Does.Contain("}"));
            }

            [Test]
            public async Task Normal_properties_that_have_empty_accessors_can_be_instrumented()
            {
                var input = await CreateInput<PropertyDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        private int magicNumber = 41;

                        public int MagicNumber
                        {            
                            get { return magicNumber + 1; }
                            set {  }
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                        input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

                var instrumentedMethodSource = SourceOfInstrumentedMember<PropertyDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[ 0], Does.Contain("public int MagicNumber"));
                Assert.That(instrumentedMethodSource[ 1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 2], Does.Contain("get"));
                Assert.That(instrumentedMethodSource[ 3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[ 5], Does.Contain("    return magicNumber + 1;"));
                Assert.That(instrumentedMethodSource[ 6], Does.Contain("}"));
                Assert.That(instrumentedMethodSource[ 8], Does.Contain("set"));
                Assert.That(instrumentedMethodSource[ 9], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[10], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[11], Does.Contain("}"));
            }
        }

        [TestFixture]
        public class Expression_bodied_properties : InstrumentationTestsBase
        {
            [Test]
            public async Task Expression_bodied_properties_can_be_instrumented()
            {
                var input = await CreateInput<PropertyDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        private static int magicNumber = 41;

                        public static int MagicNumber => magicNumber + 1;
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                        input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

                var instrumentedMethodSource = SourceOfInstrumentedMember<PropertyDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public static int MagicNumber"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain("get"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[5], Does.Contain("    return magicNumber + 1;"));
                Assert.That(instrumentedMethodSource[6], Does.Contain("}"));
            }
        
            [Test]
            public async Task Expression_bodied_properties_with_accessors_can_be_instrumented()
            {
                var input = await CreateInput<PropertyDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public static class DummyClass
                    {
                        private static int magicNumber = 41;

                        public static int MagicNumber
                        {
                            get => magicNumber + 1;
                            set => magicNumber = value + 1; }
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

                var instrumentedMethodSource = SourceOfInstrumentedMember<PropertyDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[ 0], Does.Contain("public static int MagicNumber"));
                Assert.That(instrumentedMethodSource[ 1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 2], Does.Contain("get"));
                Assert.That(instrumentedMethodSource[ 3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[ 5], Does.Contain("    return magicNumber + 1;"));
                Assert.That(instrumentedMethodSource[ 6], Does.Contain("}"));
                Assert.That(instrumentedMethodSource[ 8], Does.Contain("set"));
                Assert.That(instrumentedMethodSource[ 9], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[10], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[11], Does.Contain("    magicNumber = value + 1;"));
                Assert.That(instrumentedMethodSource[12], Does.Contain("}"));
            }

            [Test]
            public async Task Expression_bodied_properties_with_only_a_setter_can_be_instrumented()
            {
                var input = await CreateInput<PropertyDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class DummyClass
                    {
                        private int magicNumber = 41;

                        public int MagicNumber
                        {
                            set => magicNumber = value + 1;
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

                var instrumentedMethodSource = SourceOfInstrumentedMember<PropertyDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[0], Does.Contain("public int MagicNumber"));
                Assert.That(instrumentedMethodSource[1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[2], Does.Contain("set"));
                Assert.That(instrumentedMethodSource[3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[5], Does.Contain("    magicNumber = value + 1;"));
                Assert.That(instrumentedMethodSource[6], Does.Contain("}"));
            }
        }

        [Test]
        public async Task Properties_where_getter_and_setters_are_mixture_of_auto_and_normal_can_be_instrumented()
        {
            var input1 = await CreateInput<PropertyDeclarationSyntax>(@"
            namespace DummyNamespace
            {
                public static class DummyClass
                {
                    private static int magicNumber = 41;

                    public static int MagicNumber
                    {
                        get { return magicNumber + 1; }
                        set => magicNumber = value + 1;
                    }
                }
            }");
            var input2 = await CreateInput<PropertyDeclarationSyntax>(@"
            namespace DummyNamespace
            {
                public static class DummyClass
                {
                    private static int magicNumber = 41;

                    public static int MagicNumber
                    {
                        get => magicNumber + 1;
                        set { magicNumber = value + 1; }
                    }
                }
            }");

            Assert.That(input1.MemberToInstrument.CanInstrument(), Is.True);
            Assert.That(input2.MemberToInstrument.CanInstrument(), Is.True);

            var instrumentedSyntaxTree1 = await InstrumentationImpl.InstrumentDocument(
                    input1.OriginalSyntaxTree, input1.OriginalDocument, (_, __) => {}, () => 0);

            var instrumentedSyntaxTree2 = await InstrumentationImpl.InstrumentDocument(
                    input2.OriginalSyntaxTree, input2.OriginalDocument, (_, __) => {}, () => 0);

            var instrumentedMethodSources = new[]
            {
                SourceOfInstrumentedMember<PropertyDeclarationSyntax>(instrumentedSyntaxTree1),
                SourceOfInstrumentedMember<PropertyDeclarationSyntax>(instrumentedSyntaxTree2)
            };
            foreach (var instrumentedMethodSource in instrumentedMethodSources)
            {
                Assert.That(instrumentedMethodSource[ 0], Does.Contain("public static int MagicNumber"));
                Assert.That(instrumentedMethodSource[ 1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 2], Does.Contain("get"));
                Assert.That(instrumentedMethodSource[ 3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[ 5], Does.Contain("    return magicNumber + 1;"));
                Assert.That(instrumentedMethodSource[ 6], Does.Contain("}"));
                Assert.That(instrumentedMethodSource[ 8], Does.Contain("set"));
                Assert.That(instrumentedMethodSource[ 9], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[10], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[11], Does.Contain("    magicNumber = value + 1;"));
                Assert.That(instrumentedMethodSource[12], Does.Contain("}"));
            }
        }

        [TestFixture]
        public class Indexers : InstrumentationTestsBase
        {
            [Test]
            public async Task Indexers_can_be_instrumented()
            {
                var input = await CreateInput<IndexerDeclarationSyntax>(@"
                namespace DummyNamespace
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

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

                var instrumentedMethodSource = SourceOfInstrumentedMember<IndexerDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[ 0], Does.Contain("public int this[int i]"));
                Assert.That(instrumentedMethodSource[ 1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 2], Does.Contain("get"));
                Assert.That(instrumentedMethodSource[ 3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[ 5], Does.Contain("    return arr[i];"));
                Assert.That(instrumentedMethodSource[ 6], Does.Contain("}"));
                Assert.That(instrumentedMethodSource[ 8], Does.Contain("set"));
                Assert.That(instrumentedMethodSource[ 9], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[10], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[11], Does.Contain("    arr[i] = value;"));
                Assert.That(instrumentedMethodSource[12], Does.Contain("}"));
            }

            [Test]
            public async Task Indexers_with_expression_bodies_accessors_can_be_instrumented()
            {
                var input = await CreateInput<IndexerDeclarationSyntax>(@"
                namespace DummyNamespace
                {
                    public class DummyClass
                    {
                        private int[] arr = new int[100];

                        public int this[int i]
                        {
                            get => arr[i];
                            set => arr[i] = value;
                        }
                    }
                }");

                Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

                var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                    input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

                var instrumentedMethodSource = SourceOfInstrumentedMember<IndexerDeclarationSyntax>(instrumentedSyntaxTree);
                Assert.That(instrumentedMethodSource[ 0], Does.Contain("public int this[int i]"));
                Assert.That(instrumentedMethodSource[ 1], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 2], Does.Contain("get"));
                Assert.That(instrumentedMethodSource[ 3], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[ 4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[ 5], Does.Contain("    return arr[i];"));
                Assert.That(instrumentedMethodSource[ 6], Does.Contain("}"));
                Assert.That(instrumentedMethodSource[ 8], Does.Contain("set"));
                Assert.That(instrumentedMethodSource[ 9], Does.Contain("{"));
                Assert.That(instrumentedMethodSource[10], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
                Assert.That(instrumentedMethodSource[11], Does.Contain("    arr[i] = value;"));
                Assert.That(instrumentedMethodSource[12], Does.Contain("}"));
            }
        }

        [Test]
        public async Task Auto_properties_cannot_be_instrumented()
        {
            var input = await CreateInput<PropertyDeclarationSyntax>(@"
            namespace DummyNamespace
            {
                public static class DummyClass
                {
                    public static int MagicNumber { get; set; }
                }
            }");

            Assert.That(input.MemberToInstrument.CanInstrument(), Is.False);
        }
        
        [Test]
        public async Task Event_properties_can_be_instrumented()
        {
            var input = await CreateInput<EventDeclarationSyntax>(@"
            namespace DummyNamespace
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

            Assert.That(input.MemberToInstrument.CanInstrument(), Is.True);

            var instrumentedSyntaxTree = await InstrumentationImpl.InstrumentDocument(
                input.OriginalSyntaxTree, input.OriginalDocument, (_, __) => {}, () => 0);

            var instrumentedMethodSource = SourceOfInstrumentedMember<EventDeclarationSyntax>(instrumentedSyntaxTree);
            Assert.That(instrumentedMethodSource[ 0], Does.Contain("public event EventHandler<EventArgs> SomeEvent"));
            Assert.That(instrumentedMethodSource[ 1], Does.Contain("{"));
            Assert.That(instrumentedMethodSource[ 2], Does.Contain("add"));
            Assert.That(instrumentedMethodSource[ 3], Does.Contain("{"));
            Assert.That(instrumentedMethodSource[ 4], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
            Assert.That(instrumentedMethodSource[ 5], Does.Contain("    someEvent += value;"));
            Assert.That(instrumentedMethodSource[ 6], Does.Contain("}"));
            Assert.That(instrumentedMethodSource[ 8], Does.Contain("remove"));
            Assert.That(instrumentedMethodSource[ 9], Does.Contain("{"));
            Assert.That(instrumentedMethodSource[10], Does.Contain($"   System.Console.WriteLine(\"{InstrumentationImpl.CoverageOutputLinePrefix}"));
            Assert.That(instrumentedMethodSource[11], Does.Contain("    someEvent -= value;"));
            Assert.That(instrumentedMethodSource[12], Does.Contain("}"));
        }
    }
}
