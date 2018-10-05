using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class BaseMethodDeclarationSyntaxExtensions
    {
        public static bool ReturnsSomething(this BaseMethodDeclarationSyntax declaration)
        {
            TypeSyntax returnType = null;

            switch (declaration)
            {
                case OperatorDeclarationSyntax @operator: returnType = @operator.ReturnType; break;
                case ConversionOperatorDeclarationSyntax conversionOperator: returnType = conversionOperator.Type; break;
                case MethodDeclarationSyntax method: returnType = method.ReturnType; break;
            }

            if (returnType == null)
            {
                return false;
            }

            var returnsVoid = returnType is PredefinedTypeSyntax typeSyntax && typeSyntax.Keyword.Kind() == SyntaxKind.VoidKeyword;
            return !returnsVoid;
        }

        public static BaseMethodDeclarationSyntax WithBody(this BaseMethodDeclarationSyntax declaration, BlockSyntax body)
        {
            switch (declaration)
            {
                case ConstructorDeclarationSyntax constructor: return constructor.WithBody(body);
                case DestructorDeclarationSyntax destructor: return destructor.WithBody(body);
                case OperatorDeclarationSyntax @operator: return @operator.WithBody(body);
                case ConversionOperatorDeclarationSyntax conversionOperator: return conversionOperator.WithBody(body);
                case MethodDeclarationSyntax method: return method.WithBody(body);
            }
            throw new NotImplementedException();
        }

        public static BaseMethodDeclarationSyntax WithNoExpressionBody(this BaseMethodDeclarationSyntax declaration)
        {
            switch (declaration)
            {
                case ConstructorDeclarationSyntax constructor: return constructor.WithExpressionBody(null);
                case DestructorDeclarationSyntax destructor: return destructor.WithExpressionBody(null);
                case OperatorDeclarationSyntax @operator: return @operator.WithExpressionBody(null);
                case ConversionOperatorDeclarationSyntax conversionOperator: return conversionOperator.WithExpressionBody(null);
                case MethodDeclarationSyntax method: return method.WithExpressionBody(null);
            }
            throw new NotImplementedException();
        }
    }
}