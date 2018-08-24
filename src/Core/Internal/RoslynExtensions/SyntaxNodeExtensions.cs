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
            MethodDeclarationSyntax foundMember = null;

            SyntaxNode node = targetNode.Parent;
            while (node != null)
            {
                if (node is NamespaceDeclarationSyntax @namespace) foundNamespace = @namespace;
                else if (node is ClassDeclarationSyntax @class) foundClass = @class;                
                else if (node is MethodDeclarationSyntax method) foundMember = method;

                node = node.Parent;
            }
            
            if (foundNamespace != null && foundClass != null && foundMember != null)
            {                
                var parameters = string.Join(",",
                    foundMember.ParameterList.Parameters
                        .Select(p => FullyQualifiedTypeName(p.Type, semanticModel)));

                var returnType = FullyQualifiedTypeName(foundMember.ReturnType, semanticModel);

                return $"{returnType} {foundNamespace.Name}.{foundClass.Identifier}::{foundMember.Identifier}({parameters})";
            }

            return null;
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

