using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Fettle.Core.Internal
{
    internal static class TextSpanExtensions
    {
        public static string ToSourceText(this TextSpan toExtract, SyntaxNode syntaxRoot)
        {
            var allLines = SourceText.From(syntaxRoot.GetText().ToString()).Lines;

            var lineSpan = syntaxRoot.SyntaxTree.GetLineSpan(toExtract);
            var startLine = lineSpan.StartLinePosition.Line;
            var endLine = lineSpan.EndLinePosition.Line;

            if (startLine == endLine)
            {
                return allLines[startLine].ToString();
            }
            else
            {
                var extractedLines = allLines.Skip(startLine)
                                             .Take((endLine - startLine) + 1);

                return string.Join(separator: Environment.NewLine, values: extractedLines);
            }
        }
    }
}