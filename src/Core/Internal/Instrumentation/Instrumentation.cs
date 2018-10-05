using System;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Fettle.Core.Internal.Instrumentation
{
    internal static class Instrumentation
    {
        public const string CoverageOutputLinePrefix = "fettle_covered_member:";

        public static async Task<SyntaxTree> InstrumentDocument(
            SyntaxTree originalSyntaxTree,
            Document document,
            Action<string, string> onMemberInstrumented)
        {
            var root = await originalSyntaxTree.GetRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var documentEditor = DocumentEditor.CreateAsync(document).Result;
            
            foreach (var memberNode in root.DescendantNodes()
                                           .OfType<MemberDeclarationSyntax>()
                                           .Where(memberNode => memberNode.CanInstrument()))
            {
                var fullMemberName = memberNode.ChildNodes().First().NameOfContainingMember(semanticModel);
                var memberId = Guid.NewGuid().ToString();

                InstrumentMember(memberId, memberNode, documentEditor);

                onMemberInstrumented(memberId, fullMemberName);
            }
            
            return await documentEditor.GetChangedDocument().GetSyntaxTreeAsync();
        }

        private static void InstrumentMember(string methodId, MemberDeclarationSyntax memberNode, DocumentEditor documentEditor)
        {
            var instrumentationNode = SyntaxFactory.ParseStatement(
                $"System.Console.WriteLine(\"{CoverageOutputLinePrefix}{methodId}\");");

            if (memberNode is BaseMethodDeclarationSyntax baseMethodNode)
            {
                MethodInstrumentation.InstrumentMethod(baseMethodNode, instrumentationNode, documentEditor);
            }
            else if (memberNode is BasePropertyDeclarationSyntax basePropertyNode)
            {
                PropertyInstrumentation.InstrumentProperty(basePropertyNode, instrumentationNode, documentEditor);
            }
        }

        // A note on instrumenting expression-bodied methods/properties:
        //
        // We replace expression bodies (which can only have one statement) with a normal body
        // so that we can add the extra instrumentation statement.
        //
        // E.g. these:
        //
        //      public int MethodA() => 42;
        //      public void MethodB(string s) => Console.WriteLine(s);
        //
        // become these:
        //
        //      public int MethodA()
        //      {
        //          <instrumentation statement goes here>
        //          return 42;
        //      }
        //
        //      public void MethodB(string s)
        //      {
        //          <instrumentation statement goes here>
        //          Console.WriteLine(s);
        //      }
    }
}

