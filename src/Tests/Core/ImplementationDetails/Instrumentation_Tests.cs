using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class Instrumentation_Tests
    {
        [Test]
        public async Task Normal_methods_can_be_instrumented()
        {
            const string originalSource = @"
namespace DummyNamespace
{
    public class DummyClass
    {
        public int MagicNumber(int a)
        {
            return 42;
        }
    }
}
";
            var originalDocument = SourceToDocument(originalSource);
            var originalSyntaxTree = await originalDocument.GetSyntaxTreeAsync();

            var instrumentedSyntaxTree = await Instrumentation.InstrumentDocument(originalSyntaxTree, originalDocument, (_, __) => {});
            
            var instrumentedMethodSource = InstrumentedMethodSource(instrumentedSyntaxTree);
            Assert.That(instrumentedMethodSource, Does.Contain("public int MagicNumber(int a)"));
            Assert.That(instrumentedMethodSource, Does.Contain("{"));
            Assert.That(instrumentedMethodSource, Does.Contain($"   System.Console.WriteLine(\"{Instrumentation.CoverageOutputLinePrefix}"));
            Assert.That(instrumentedMethodSource, Does.Contain("    return 42;"));
            Assert.That(instrumentedMethodSource, Does.Contain("}"));
        }

        [Test]
        public async Task Empty_normal_methods_can_be_instrumented()
        {
            const string originalSource = @"
namespace DummyNamespace
{
    public class DummyClass
    {
        public void DoSomething()
        {
        }
    }
}
";
            var originalDocument = SourceToDocument(originalSource);
            var originalSyntaxTree = await originalDocument.GetSyntaxTreeAsync();

            var instrumentedSyntaxTree = await Instrumentation.InstrumentDocument(originalSyntaxTree, originalDocument, (_, __) => {});
            
            var instrumentedMethodSource = InstrumentedMethodSource(instrumentedSyntaxTree);
            Assert.That(instrumentedMethodSource, Does.Contain("public void DoSomething()"));
            Assert.That(instrumentedMethodSource, Does.Contain("{"));
            Assert.That(instrumentedMethodSource, Does.Contain($"   System.Console.WriteLine(\"{Instrumentation.CoverageOutputLinePrefix}"));
            Assert.That(instrumentedMethodSource, Does.Contain("}"));
        }

        [Test]
        public async Task Expression_bodied_methods_that_return_a_value_can_be_instrumented()
        {
            const string originalSource = @"
namespace DummyNamespace
{
    public class DummyClass
    {
        public int MagicNumber(int a) => 42;
    }
}
";
            var originalDocument = SourceToDocument(originalSource);
            var originalSyntaxTree = await originalDocument.GetSyntaxTreeAsync();

            var instrumentedSyntaxTree = await Instrumentation.InstrumentDocument(originalSyntaxTree, originalDocument, (_, __) => {});
            
            var instrumentedMethodSource = InstrumentedMethodSource(instrumentedSyntaxTree);
            Assert.That(instrumentedMethodSource, Does.Contain("public int MagicNumber(int a)"));
            Assert.That(instrumentedMethodSource, Does.Contain("{"));
            Assert.That(instrumentedMethodSource, Does.Contain($"   System.Console.WriteLine(\"{Instrumentation.CoverageOutputLinePrefix}"));
            Assert.That(instrumentedMethodSource, Does.Contain("    return 42;"));
            Assert.That(instrumentedMethodSource, Does.Contain("}"));
        }

        [Test]
        public async Task Expression_bodied_methods_that_do_not_return_a_value_can_be_instrumented()
        {
            const string originalSource = @"
namespace DummyNamespace
{
    public class DummyClass
    {
        public void DoSomething() => System.Console.WriteLine(""hello"");
    }
}
";
            var originalDocument = SourceToDocument(originalSource);
            var originalSyntaxTree = await originalDocument.GetSyntaxTreeAsync();

            var instrumentedSyntaxTree = await Instrumentation.InstrumentDocument(originalSyntaxTree, originalDocument, (_, __) => {});
            
            var instrumentedMethodSource = InstrumentedMethodSource(instrumentedSyntaxTree);
            Assert.That(instrumentedMethodSource, Does.Contain("public void DoSomething()"));
            Assert.That(instrumentedMethodSource, Does.Contain("{"));
            Assert.That(instrumentedMethodSource, Does.Contain($"   System.Console.WriteLine(\"{Instrumentation.CoverageOutputLinePrefix}"));
            Assert.That(instrumentedMethodSource, Does.Contain("    System.Console.WriteLine(\"hello\");"));
            Assert.That(instrumentedMethodSource, Does.Contain("}"));
        }

        [Test(Description = "fix for regression of issue #27")]
        public async Task Expression_bodied_methods_with_return_type_that_is_not_predefined_type_can_be_instrumented()
        {
            const string originalSource = @"
namespace DummyNamespace
{
    public class DummyClass
    {
        public Task<System.Generic.List<int>> GetThings() => new[]{1,2,3}.ToList();
    }
}
";
            var originalDocument = SourceToDocument(originalSource);
            var originalSyntaxTree = await originalDocument.GetSyntaxTreeAsync();

            var instrumentedSyntaxTree = await Instrumentation.InstrumentDocument(originalSyntaxTree, originalDocument, (_, __) => {});
            
            var instrumentedMethodSource = InstrumentedMethodSource(instrumentedSyntaxTree);
            Assert.That(instrumentedMethodSource, Does.Contain("public Task<System.Generic.List<int>> GetThings()"));
            Assert.That(instrumentedMethodSource, Does.Contain("{"));
            Assert.That(instrumentedMethodSource, Does.Contain($"   System.Console.WriteLine(\"{Instrumentation.CoverageOutputLinePrefix}"));
            Assert.That(instrumentedMethodSource, Does.Contain("   return new[]{1, 2, 3}.ToList();"));
            Assert.That(instrumentedMethodSource, Does.Contain("}"));
        }

        [Test]
        public async Task The_callback_is_called_once_for_each_instrumented_method()
        {
            const string originalSource = @"
namespace DummyNamespace
{
    public class DummyClass
    {
        public int MethodA(int a) { return 42; }
        public void MethodB() { }
        public int MethodC() => 42;
        public void MethodD(string s) => System.Console.WriteLine(s);
    }
}
";
            var originalDocument = SourceToDocument(originalSource);
            var originalSyntaxTree = await originalDocument.GetSyntaxTreeAsync();
            var received = new List<Tuple<string, string>>();

            await Instrumentation.InstrumentDocument(
                originalSyntaxTree,
                originalDocument, 
                (methodId, fullMethodName) => received.Add(new Tuple<string,string>(methodId, fullMethodName)));
            
            Assert.That(received.Count, Is.EqualTo(4));
            Assert.That(received[0].Item2, Does.Contain("MethodA"));
            Assert.That(received[1].Item2, Does.Contain("MethodB"));
            Assert.That(received[2].Item2, Does.Contain("MethodC"));
            Assert.That(received[3].Item2, Does.Contain("MethodD"));
        }

        private static string InstrumentedMethodSource(SyntaxTree instrumentedSyntaxTree)
        {
            var node = instrumentedSyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single()
                .NormalizeWhitespace();
            
            return node.ToString();
        }

        private static Document SourceToDocument(string source)
        {
            using (var ws = new AdhocWorkspace())
            {
                var emptyProject = ws.AddProject(
                    ProjectInfo.Create(
                        ProjectId.CreateNewId(),
                        VersionStamp.Default,
                        "test",
                        "test.dll",
                        LanguageNames.CSharp));

                return emptyProject.AddDocument("test.cs", source);
            }
        }
    }
}
