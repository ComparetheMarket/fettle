using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Fettle.Core.Internal.Mutators
{    
    internal class InvertIfStatementConditionMutator : IMutator
    {
        public SyntaxNode Mutate(SyntaxNode node)
        {
            var ifStatement = (IfStatementSyntax)node;
            
            if (ifStatement.Condition is PrefixUnaryExpressionSyntax prefixUnarySyntax && 
                ifStatement.Condition.Kind() == SyntaxKind.LogicalNotExpression)
            {
                var mutatedCondition = prefixUnarySyntax.Operand;
                return ifStatement.WithCondition(mutatedCondition);
            }
            else
            {
                var mutatedCondition = SyntaxFactory.ParenthesizedExpression(ifStatement.Condition)
                    .WithAdditionalAnnotations(Simplifier.Annotation);

                var mutatedExpression = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, mutatedCondition);

                return ifStatement.WithCondition(mutatedExpression);
            }
        }
    }
}
