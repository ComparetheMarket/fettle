using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class MemberDeclarationSyntaxExtensions
    {
        public static bool CanInstrument(this MemberDeclarationSyntax memberDeclaration)
        {
            if (memberDeclaration is BaseMethodDeclarationSyntax methodDeclaration)
            {
                return CanInstrumentMethod(methodDeclaration);
            }
            else if (memberDeclaration is BasePropertyDeclarationSyntax propertyDeclaration)
            {
                return CanInstrumentProperty(propertyDeclaration);
            }

            return false;
        }

        private static bool CanInstrumentMethod(BaseMethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Body != null || methodDeclaration.ExpressionBody != null;
        }

        private static bool CanInstrumentProperty(BasePropertyDeclarationSyntax propertyDeclaration)
        {
            var hasAccessors = propertyDeclaration.AccessorList != null;
            if (hasAccessors)
            {
                var accessors = propertyDeclaration.AccessorList.ChildNodes().OfType<AccessorDeclarationSyntax>().ToArray();
                var areAnyAutoAccessors = accessors.Any(a => a.Body == null && a.ExpressionBody == null);

                // Assumption: you can't mix auto accessors and non-auto accessors in a single property.
                // E.g. these won't compile:
                //      int Thing { get; set => x = value; }
                //      int Thing { get { return x; } set; }
                // Therefore, if a getter has a body/expression body then the setter will too (and vice versa).
                // Therefore we only need to check one accessor.
                // (And the same with add/remove accessors for events).

                if (areAnyAutoAccessors)
                {
                    // Auto-accessor means that there is no body/expression body, so nothing to mutate
                    return false;
                }
            }

            return true;
        }
    }
}
