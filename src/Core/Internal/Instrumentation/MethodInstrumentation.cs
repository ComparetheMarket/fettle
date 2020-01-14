using System.Linq;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Fettle.Core.Internal.Instrumentation
{
    internal static class MethodInstrumentation
    {
        public static void InstrumentMethod(
            BaseMethodDeclarationSyntax methodNode,
            StatementSyntax instrumentationNode,
            DocumentEditor documentEditor)
        {
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
            BaseMethodDeclarationSyntax methodNode,
            SyntaxEditor syntaxEditor,
            StatementSyntax instrumentationNode)
        {
            var firstChildNode = methodNode.Body.ChildNodes().FirstOrDefault();
            var isMethodEmpty = firstChildNode == null;
            if (isMethodEmpty)
            {
                syntaxEditor.ReplaceNode(
                    methodNode,
                    methodNode.WithBody(SyntaxFactory.Block(instrumentationNode)));
            }
            else
            {
                syntaxEditor.InsertBefore(firstChildNode, instrumentationNode);
            }
        }
        private static void InstrumentExpressionBodiedMethod(
            BaseMethodDeclarationSyntax methodNode,
            SyntaxEditor syntaxEditor,
            StatementSyntax instrumentationNode)
        {
            BlockSyntax newMethodBodyBlock;

            if (methodNode.ReturnsSomething())
            {
                newMethodBodyBlock = SyntaxFactory.Block(
                    instrumentationNode,
                    SyntaxFactory.ReturnStatement(methodNode.ExpressionBody.Expression));
            }
            else
            {
                newMethodBodyBlock = SyntaxFactory.Block(
                    instrumentationNode,
                    SyntaxFactory.ExpressionStatement(methodNode.ExpressionBody.Expression));
            }

            var newMethodNode = methodNode
                .WithNoExpressionBody()
                .WithBody(newMethodBodyBlock);

            syntaxEditor.ReplaceNode(methodNode, newMethodNode);
        }
    }
}