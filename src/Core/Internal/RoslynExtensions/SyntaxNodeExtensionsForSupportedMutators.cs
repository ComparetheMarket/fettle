using System.Collections.Generic;
using System.Linq;
using Fettle.Core.Internal.Mutators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class SyntaxNodeExtensionsForSupportedMutators
    {
        public static IList<IMutator> SupportedMutators(this SyntaxNode node)
        {
            switch (node)
            {
                case BinaryExpressionSyntax binaryExpression:
                    return MutatorsThatModifyBinaryExpressions(binaryExpression);

                case PrefixUnaryExpressionSyntax prefixUnaryExpression:
                    return MutatorsThatSwapUnaryExpressions(prefixUnaryExpression.OperatorToken);

                case PostfixUnaryExpressionSyntax postfixUnaryExpression:
                    return MutatorsThatSwapUnaryExpressions(postfixUnaryExpression.OperatorToken);

                case AssignmentExpressionSyntax assignmentExpression:
                    return MutatorsThatReplaceAssignmentOperators(assignmentExpression);

                case ConditionalExpressionSyntax _:
                    return new List<IMutator> { new InvertConditionalExpressionMutator() };

                case IfStatementSyntax _:
                    return new List<IMutator> { new InvertIfStatementConditionMutator() };
            }

            return new List<IMutator>();
        }

        private static IList<IMutator> MutatorsThatReplaceAssignmentOperators(AssignmentExpressionSyntax assignmentExpression)
        {
            var operatorFrom = assignmentExpression.OperatorToken.ToString();
            return MutatorsThatReplaceOperators(operatorFrom,
                new[]
                {
                    new[] {"+=", "-=", "*=", "/=", "%="}
                });
        }

        private static IList<IMutator> MutatorsThatSwapUnaryExpressions(SyntaxToken operatorToken)
        {
            return MutatorsThatReplaceOperators(operatorToken.ToString(), new[] { new[] { "--", "++" } });
        }

        private static IList<IMutator> MutatorsThatModifyBinaryExpressions(BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.Kind() == SyntaxKind.CoalesceExpression)
            {
                return new List<IMutator> { new InvertNullCoalescingOperatorMutator() };
            }
            else
            {
                var operatorFrom = binaryExpression.OperatorToken.ToString();
                return MutatorsThatReplaceOperators(operatorFrom,
                    new[]
                    {
                        new[] {"+", "-", "*", "/", "%"},
                        new[] {">", "<", ">=", "<="},
                        new[] {"==", "!="},
                        new[] {"&&", "||"}
                    });
            }
        }

        private static IList<IMutator> MutatorsThatReplaceOperators(string operatorFrom, string[][] operatorSets)
        {
            var operatorSet = operatorSets.SingleOrDefault(o => o.Contains(operatorFrom));
            if (operatorSet == null)
            {
                return new List<IMutator>();
            }

            return operatorSet.Except(new[] { operatorFrom })
                .Select(operatorTo => new ReplaceOperatorMutator(operatorFrom, operatorTo))
                .Cast<IMutator>()
                .ToList();
        }
    }
}
