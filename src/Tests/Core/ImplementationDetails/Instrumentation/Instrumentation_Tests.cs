using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstrumentationImpl = Fettle.Core.Internal.Instrumentation.Instrumentation;

namespace Fettle.Tests.Core.ImplementationDetails.Instrumentation
{
    [TestFixture]
    class Instrumentation_Tests : InstrumentationTestsBase
    {
        [Test]
        public async Task The_callback_is_called_once_for_each_instrumented_method()
        {
            var input = await CreateInput<MemberDeclarationSyntax>(@"
            namespace DummyNamespace
            {
                public class DummyClass
                {
                    public int MethodA(int a) { return 42; }
                    public void MethodB() { }
                    public int MethodC() => 42;
                    public void MethodD(string s) => System.Console.WriteLine(s);
                }
            }");
            var received = new List<Tuple<string, string>>();

            await InstrumentationImpl.InstrumentDocument(
                input.OriginalSyntaxTree,
                input.OriginalDocument, 
                (methodId, fullMethodName) => received.Add(new Tuple<string,string>(methodId, fullMethodName)));

            Assert.That(received.Count, Is.EqualTo(4));
            Assert.That(received[0].Item2, Does.Contain("MethodA"));
            Assert.That(received[1].Item2, Does.Contain("MethodB"));
            Assert.That(received[2].Item2, Does.Contain("MethodC"));
            Assert.That(received[3].Item2, Does.Contain("MethodD"));
        }
    }
}
