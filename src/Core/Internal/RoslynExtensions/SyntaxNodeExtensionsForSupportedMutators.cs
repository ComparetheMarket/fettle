using System.Collections.Generic;
using System.Linq;
using Fettle.Core.Internal.Mutators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class SyntaxNodeExtensionsForSupportedMutators
    {
        public static IList<IMutator> SupportedMutators(this SyntaxNode node)
        {
            if (node is BinaryExpressionSyntax binaryExpression)
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
            else if (node is PrefixUnaryExpressionSyntax prefixUnaryExpression)
            {
                var operatorFrom = prefixUnaryExpression.OperatorToken.ToString();
                return MutatorsThatReplaceOperators(operatorFrom, new[] { new[]{ "--", "++" } });
            }
            else if (node is PostfixUnaryExpressionSyntax postfixUnaryExpression)
            {
                var operatorFrom = postfixUnaryExpression.OperatorToken.ToString();
                return MutatorsThatReplaceOperators(operatorFrom, new[] { new[]{ "--", "++" } });
            }
            else if (node is ConditionalExpressionSyntax)
            {
                return new List<IMutator> { new InvertConditionalExpressionMutator() };
            }
            
            return new List<IMutator>();
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
