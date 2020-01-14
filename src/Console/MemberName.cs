using System;
using System.Linq;

namespace Fettle.Console
{
    internal static class MemberName
    {
        public static string Simplify(string fullMemberName)
        {
            var tokensInReverseOrder = fullMemberName.Split(new[] { "::" }, StringSplitOptions.None).Reverse().ToArray();
            var memberNameWithoutParens = tokensInReverseOrder.First().Split('(').First();
            var className = tokensInReverseOrder.Skip(1).First().Split('.').Last();

            return $"{className}.{memberNameWithoutParens}";
        }
    }
}
