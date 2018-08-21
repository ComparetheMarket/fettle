using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class MethodDeclarationSyntaxExtensions
    {
        public static bool CanInstrument(this MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Body != null || methodDeclaration.ExpressionBody != null;
        }
    }
}
