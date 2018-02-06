using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.Mutators
{
    internal class InvertConditionalExpressionMutator : IMutator
    {
        public SyntaxNode Mutate(SyntaxNode node)
        {
            var conditionalExpression = (ConditionalExpressionSyntax)node;
            
            return SyntaxFactory.ConditionalExpression(
                    conditionalExpression.Condition, 
                    whenTrue: conditionalExpression.WhenFalse,
                    whenFalse: conditionalExpression.WhenTrue)
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia())
                .NormalizeWhitespace();
        }
    }
}