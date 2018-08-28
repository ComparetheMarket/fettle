using System;
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
            throw new NotImplementedException();
        }
    }
}
