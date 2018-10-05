using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class SyntaxNodeExtensions
    {        
        public static string NameOfContainingMember(this SyntaxNode targetNode, SemanticModel semanticModel)
        {
            NamespaceDeclarationSyntax foundNamespace = null;
            ClassDeclarationSyntax foundClass = null;
            BaseMethodDeclarationSyntax foundMethod = null;
            PropertyDeclarationSyntax foundProperty = null;

            SyntaxNode node = targetNode.Parent;
            while (node != null)
            {
                if (node is NamespaceDeclarationSyntax @namespace) foundNamespace = @namespace;
                else if (node is ClassDeclarationSyntax @class) foundClass = @class;                
                else if (node is BaseMethodDeclarationSyntax method) foundMethod = method;
                else if (node is PropertyDeclarationSyntax property) foundProperty = property;

                node = node.Parent;
            }
            
            if (foundNamespace != null && foundClass != null)
            {
                if (foundMethod != null)
                {
                    var parameters = string.Join(",",
                        foundMethod.ParameterList.Parameters
                            .Select(p => FullyQualifiedTypeName(p.Type, semanticModel)));

                    var returnType = ReturnType(semanticModel, foundMethod);
                    var returnTypeFormatted = returnType != null ? $"{returnType} " : "";

                    var identifier = Identifier(foundMethod);

                    return $"{returnTypeFormatted}{foundNamespace.Name}.{foundClass.Identifier}::{identifier}({parameters})";
                }
                else if (foundProperty != null)
                {
                    var returnType = FullyQualifiedTypeName(foundProperty.Type, semanticModel);
                    return $"{returnType} {foundNamespace.Name}.{foundClass.Identifier}::{foundProperty.Identifier}";
                }
            }

            return null;
        }

        private static string Identifier(BaseMethodDeclarationSyntax baseMethod)
        {
            switch (baseMethod)
            {
                case ConstructorDeclarationSyntax constructor: return constructor.Identifier.ToString();
                case DestructorDeclarationSyntax destructor: return $"~{destructor.Identifier}";
                case MethodDeclarationSyntax method: return method.Identifier.ToString();
                case OperatorDeclarationSyntax @operator: return $"operator {@operator.OperatorToken}";
                case ConversionOperatorDeclarationSyntax conversionOperator: return $"operator {conversionOperator.Type}";
            }
            throw new NotImplementedException();
        }

        private static string ReturnType(SemanticModel semanticModel, BaseMethodDeclarationSyntax baseMethod)
        {
            switch (baseMethod)
            {
                case ConstructorDeclarationSyntax _: 
                case DestructorDeclarationSyntax _: 
                case OperatorDeclarationSyntax _: 
                case ConversionOperatorDeclarationSyntax _: 
                    return null;

                case MethodDeclarationSyntax method: return FullyQualifiedTypeName(method.ReturnType, semanticModel);
            }
            throw new NotImplementedException();
        }

        private static string FullyQualifiedTypeName(TypeSyntax type, SemanticModel semanticModel)
        {
            var typeInfoType = semanticModel.GetTypeInfo(type).Type;

            var symbolDisplayFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            return typeInfoType.ToDisplayString(symbolDisplayFormat);
        }
    }
}



