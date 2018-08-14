using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Fettle.Core.Internal
{
    internal static class TextSpanExtensions
    {
        public static string ToSourceText(this TextSpan toExtract, SyntaxNode syntaxRoot)
        {
            var originalLineNumber = syntaxRoot.SyntaxTree.GetLineSpan(toExtract)
                .StartLinePosition.Line;

            var entireSource = SourceText.From(syntaxRoot.GetText().ToString());
            return entireSource.Lines[originalLineNumber].ToString();
        }
    }
}