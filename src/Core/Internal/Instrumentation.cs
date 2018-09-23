using System;
using System.Collections.Generic;
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

            if (memberNode is MethodDeclarationSyntax methodNode)
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
            else if (memberNode is PropertyDeclarationSyntax propertyNode)
            {
                if (propertyNode.ExpressionBody != null)
                {
                    InstrumentExpressionBodiedProperty(propertyNode, documentEditor, instrumentationNode);
                }
                else
                {
                    foreach (var accessorNode in propertyNode.AccessorList.Accessors)
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

        private static void InstrumentNormalPropertyAccessor(
            AccessorDeclarationSyntax propertyAccessorNode, 
            DocumentEditor documentEditor, 
            StatementSyntax instrumentationNode)
        {
            var firstChildNode = propertyAccessorNode.Body.ChildNodes().FirstOrDefault();
            var isAccessorEmpty = firstChildNode == null;
            if (isAccessorEmpty)
            {
                documentEditor.ReplaceNode(
                    propertyAccessorNode,
                    propertyAccessorNode.WithBody(SyntaxFactory.Block(instrumentationNode)));
            }
            else
            {
                documentEditor.InsertBefore(firstChildNode, instrumentationNode);
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

        private static void InstrumentExpressionBodiedMethod(
            MethodDeclarationSyntax methodNode,
            DocumentEditor documentEditor,
            StatementSyntax instrumentationNode)
        {
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
            DocumentEditor documentEditor,
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

            documentEditor.ReplaceNode(propertyAccessorNode, newAccessorNode);
        }
    }
}
