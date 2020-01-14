using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.Mutators
{
    internal class InvertNullCoalescing : IMutator
    {
        public SyntaxNode Mutate(SyntaxNode node)
        {
            var originalExpression = (BinaryExpressionSyntax)node;

            return SyntaxFactory.BinaryExpression(
                    SyntaxKind.CoalesceExpression, 
                    left: originalExpression.Right, 
                    right: originalExpression.Left)
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia())
                .NormalizeWhitespace();
        }
    }
}