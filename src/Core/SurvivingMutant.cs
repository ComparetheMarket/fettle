using System.Threading.Tasks;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core
{
    public class SurvivingMutant : Mutant
    {
        internal static async Task<SurvivingMutant> Create(
            Document originalDocument,
            SyntaxNode originalNode,
            SyntaxNode mutatedSyntaxRoot)
        {
            var originalSyntaxRoot = await originalDocument.GetSyntaxRootAsync();

            var originalLineNumber = originalSyntaxRoot.SyntaxTree.GetLineSpan(originalNode.Span)
                .StartLinePosition.Line;

            return new SurvivingMutant
            {
                SourceFilePath = originalDocument.FilePath,
                SourceLine = originalLineNumber + 1,
                OriginalLine = SyntaxNodeToSourceText(originalNode, originalSyntaxRoot),
                MutatedLine = SyntaxNodeToSourceText(originalNode, mutatedSyntaxRoot)
            };
        }

        private static string SyntaxNodeToSourceText(SyntaxNode node, SyntaxNode syntaxRoot)
        {
            if (node is IfStatementSyntax ifStatement)
            {
                return IfStatementToSourceTextWithoutBody(ifStatement, syntaxRoot);
            }
            else
            {
                return node.Span.ToSourceText(syntaxRoot);
            }
        }

        private static string IfStatementToSourceTextWithoutBody(IfStatementSyntax ifStatement, SyntaxNode syntaxRoot)
        {
            return ifStatement.Condition.Span.ToSourceText(syntaxRoot);
        }
    }
}