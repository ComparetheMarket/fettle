using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Fettle.Core.Internal
{
    internal static class Ignoring
    {
        public static bool NodeHasBeginIgnoreComment(SyntaxNode node)
        {
            return LeadingSingleLineComments(node).Any(c =>
                c.ToString().ToLowerInvariant().Contains("fettle: begin ignore"));
        }

        public static bool NodeHasEndIgnoreComment(SyntaxNode node)
        {            
            return LeadingSingleLineComments(node).Any(c => 
                c.ToString().ToLowerInvariant().Contains("fettle: end ignore"));
        }

        private static IEnumerable<SyntaxTrivia> LeadingSingleLineComments(SyntaxNode node)
        {
            return node.GetLeadingTrivia()
                .Where(t => t.Kind() == SyntaxKind.SingleLineCommentTrivia);
        }
    }
}
