using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Fettle.Core.Internal.Instrumentation
{
    internal static class PropertyInstrumentation
    {
        public static void InstrumentProperty(
            BasePropertyDeclarationSyntax basePropertyNode,
            StatementSyntax instrumentationNode,
            DocumentEditor documentEditor)
        {
            if (basePropertyNode is PropertyDeclarationSyntax propertyNode && propertyNode.ExpressionBody != null)
            {
                InstrumentExpressionBodiedProperty(propertyNode, documentEditor, instrumentationNode);
            }
            else
            {
                foreach (var accessorNode in basePropertyNode.AccessorList.Accessors)
                {
                    var isAccessorExpressionBodied = accessorNode.ExpressionBody != null;
                    if (isAccessorExpressionBodied)
                    {
                        InstrumentExpressionBodiedPropertyAccessor(
                            accessorNode, documentEditor, instrumentationNode);
                    }
                    else
                    {
                        InstrumentNormalPropertyAccessor(
                            accessorNode, documentEditor, instrumentationNode);
                    }
                }
            }
        }

        private static void InstrumentNormalPropertyAccessor(
            AccessorDeclarationSyntax propertyAccessorNode, 
            SyntaxEditor syntaxEditor, 
            StatementSyntax instrumentationNode)
        {
            var firstChildNode = propertyAccessorNode.Body.ChildNodes().FirstOrDefault();
            var isAccessorEmpty = firstChildNode == null;
            if (isAccessorEmpty)
            {
                syntaxEditor.ReplaceNode(
                    propertyAccessorNode,
                    propertyAccessorNode.WithBody(SyntaxFactory.Block(instrumentationNode)));
            }
            else
            {
                syntaxEditor.InsertBefore(firstChildNode, instrumentationNode);
            }
        }

        private static void InstrumentExpressionBodiedProperty(
            PropertyDeclarationSyntax propertyNode,
            DocumentEditor documentEditor,
            StatementSyntax instrumentationNode)
        {
            var newGetAccessorBodyBlock = SyntaxFactory.Block(
                instrumentationNode,
                SyntaxFactory.ReturnStatement(propertyNode.ExpressionBody.Expression));

            var newPropertyNode = propertyNode
                .WithExpressionBody(null)
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, newGetAccessorBodyBlock));

            documentEditor.ReplaceNode(propertyNode, newPropertyNode);
        }

        private static void InstrumentExpressionBodiedPropertyAccessor(
            AccessorDeclarationSyntax propertyAccessorNode,
            SyntaxEditor syntaxEditor,
            StatementSyntax instrumentationNode)
        {
            BlockSyntax newAccessorBodyBlock;

            if (propertyAccessorNode.Kind() == SyntaxKind.GetAccessorDeclaration)
            {
                newAccessorBodyBlock = SyntaxFactory.Block(
                    instrumentationNode,
                    SyntaxFactory.ReturnStatement(propertyAccessorNode.ExpressionBody.Expression));
            }
            else
            {
                newAccessorBodyBlock = SyntaxFactory.Block(
                    instrumentationNode,
                    SyntaxFactory.ExpressionStatement(propertyAccessorNode.ExpressionBody.Expression));
            }

            var newAccessorNode = propertyAccessorNode.Update(
                attributeLists: propertyAccessorNode.AttributeLists,
                modifiers: propertyAccessorNode.Modifiers,
                keyword: propertyAccessorNode.Keyword,
                body: newAccessorBodyBlock,
                expressionBody: null,
                semicolonToken: new SyntaxToken());

            syntaxEditor.ReplaceNode(propertyAccessorNode, newAccessorNode);
        }
    }
}