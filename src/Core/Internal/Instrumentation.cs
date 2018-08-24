﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Fettle.Core.Internal
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

            foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                foreach (var memberNode in classNode.DescendantNodes()
                                                    .OfType<MethodDeclarationSyntax>()
                                                    .Where(methodNode => methodNode.CanInstrument()))
                {
                    var fullMemberName = memberNode.ChildNodes().First().NameOfContainingMember(semanticModel);
                    var memberId = Guid.NewGuid().ToString();

                    InstrumentMember(memberId, memberNode, documentEditor);

                    onMemberInstrumented(memberId, fullMemberName);
                }
            }

            return await documentEditor.GetChangedDocument().GetSyntaxTreeAsync();
        }

        private static void InstrumentMember(string methodId, MethodDeclarationSyntax methodNode, DocumentEditor documentEditor)
        {
            var instrumentationNode = SyntaxFactory.ParseStatement(
                $"System.Console.WriteLine(\"{CoverageOutputLinePrefix}{methodId}\");");

            var isMethodExpressionBodied = methodNode.ExpressionBody != null;
            if (isMethodExpressionBodied)
            {
                InstrumentExpressionBodiedMethod(methodNode, documentEditor, instrumentationNode);
            }
            else
            {
                InstrumentNormalMethod(methodNode, documentEditor, instrumentationNode);
            }
        }

        private static void InstrumentNormalMethod(
            MethodDeclarationSyntax methodNode,
            DocumentEditor documentEditor,
            StatementSyntax instrumentationNode)
        {
            var firstChildNode = methodNode.Body.ChildNodes().FirstOrDefault();
            var isMethodEmpty = firstChildNode == null;
            if (isMethodEmpty)
            {
                documentEditor.ReplaceNode(
                    methodNode,
                    methodNode.WithBody(SyntaxFactory.Block(instrumentationNode)));
            }
            else
            {
                documentEditor.InsertBefore(firstChildNode, instrumentationNode);
            }
        }

        private static void InstrumentExpressionBodiedMethod(
            MethodDeclarationSyntax methodNode,
            DocumentEditor documentEditor,
            StatementSyntax instrumentationNode)
        {
            // Replace expression body (which can only have one statement) with a normal method body
            // so that we can add the extra instrumentation statement.
            //
            // E.g. this:
            //
            //      public int MagicNumber() => 42;
            //
            // becomes this:
            //
            //      public int MagicNumber()
            //      {
            //          <instrumentation statement goes here>
            //          return 42;
            //      }

            BlockSyntax newMethodBodyBlock;

            var isVoidMethod = methodNode.ReturnType is PredefinedTypeSyntax typeSyntax &&
                               typeSyntax.Keyword.Kind() == SyntaxKind.VoidKeyword;

            if (isVoidMethod)
            {
                newMethodBodyBlock = SyntaxFactory.Block(
                    instrumentationNode,
                    SyntaxFactory.ExpressionStatement(methodNode.ExpressionBody.Expression));
            }
            else
            {
                newMethodBodyBlock = SyntaxFactory.Block(
                    instrumentationNode,
                    SyntaxFactory.ReturnStatement(methodNode.ExpressionBody.Expression));
            }

            var newMethodNode = methodNode
                .WithExpressionBody(null)
                .WithBody(newMethodBodyBlock);

            documentEditor.ReplaceNode(methodNode, newMethodNode);

        }
    }
}
