using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{
    internal interface IMutator
    {
        SyntaxNode Mutate(SyntaxNode node);
    }
}