using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class MethodDeclarationSyntaxExtensions
    {
        public static bool CanInstrument(this MethodDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Body != null || classDeclaration.ExpressionBody != null;
        }
    }
}
