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
            MemberDeclarationSyntax foundMember = null;

            SyntaxNode node = targetNode.Parent;
            while (node != null)
            {
                if (node is NamespaceDeclarationSyntax @namespace) foundNamespace = @namespace;
                else if (node is ClassDeclarationSyntax @class) foundClass = @class;                
                else if (node is MemberDeclarationSyntax member) foundMember = member;
                
                node = node.Parent;
            }
            
            if (foundNamespace != null && foundClass != null)
            {
                if (foundMember != null)
                {
                    var formattedParameters = FormattedParameters(semanticModel, foundMember);
                    
                    var returnType = ReturnType(semanticModel, foundMember);
                    var returnTypeFormatted = returnType != null ? $"{returnType} " : "";

                    var identifier = Identifier(foundMember);

                    return $"{returnTypeFormatted}{foundNamespace.Name}.{foundClass.Identifier}::{identifier}{formattedParameters}";
                }
            }

            return null;
        }

        private static string FormattedParameters(SemanticModel semanticModel, MemberDeclarationSyntax baseMember)
        {
            string ParametersAsString(SeparatedSyntaxList<ParameterSyntax> parameters) =>
                string.Join(",",
                    parameters.Select(p => FullyQualifiedTypeName(p.Type, semanticModel)));

            switch (baseMember)
            {
                case BaseMethodDeclarationSyntax method: return $"({ParametersAsString(method.ParameterList.Parameters)})";
                case IndexerDeclarationSyntax indexer:  return $"[{ParametersAsString(indexer.ParameterList.Parameters)}]";
            }

            return "";
        }

        private static string Identifier(MemberDeclarationSyntax baseMember)
        {
            switch (baseMember)
            {
                case ConstructorDeclarationSyntax constructor: return constructor.Identifier.ToString();
                case DestructorDeclarationSyntax destructor: return $"~{destructor.Identifier}";
                case MethodDeclarationSyntax method: return method.Identifier.ToString();
                case OperatorDeclarationSyntax @operator: return $"operator {@operator.OperatorToken}";
                case ConversionOperatorDeclarationSyntax conversionOperator: return $"operator {conversionOperator.Type}";
                
                case PropertyDeclarationSyntax property: return property.Identifier.ToString();
                case IndexerDeclarationSyntax _: return "this";
                case EventDeclarationSyntax @event: return @event.Identifier.ToString();
                
            }
            throw new NotImplementedException($"Unsupported member type: {baseMember.GetType()}");
        }

        private static string ReturnType(SemanticModel semanticModel, MemberDeclarationSyntax baseMember)
        {
            switch (baseMember)
            {
                case ConstructorDeclarationSyntax _: 
                case DestructorDeclarationSyntax _: 
                case OperatorDeclarationSyntax _: 
                case ConversionOperatorDeclarationSyntax _: 
                    return null;

                case MethodDeclarationSyntax method: return FullyQualifiedTypeName(method.ReturnType, semanticModel);
                case BasePropertyDeclarationSyntax property: return FullyQualifiedTypeName(property.Type, semanticModel);
            }
            throw new NotImplementedException($"Unsupported member type: {baseMember.GetType()}");
        }

        private static string FullyQualifiedTypeName(TypeSyntax type, SemanticModel semanticModel)
        {
            var typeInfoType = semanticModel.GetTypeInfo(type).Type;

            var symbolDisplayFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

            return typeInfoType.ToDisplayString(symbolDisplayFormat);
        }
    }
}



