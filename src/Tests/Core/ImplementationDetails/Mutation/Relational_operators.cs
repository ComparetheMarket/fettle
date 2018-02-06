using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    class Relational_operators : Contexts.BinaryExpressionMutators
    {
        [Test]
        public void Relational_operators_are_swappable()
        {
            AssertBinaryExpressionMutatesTo(">",  new[] { "<", ">=", "<=" });
            AssertBinaryExpressionMutatesTo("<",  new[] { ">", "<=", ">=" });
            AssertBinaryExpressionMutatesTo(">=", new[] { "<=", ">", "<" });
            AssertBinaryExpressionMutatesTo("<=", new[] { ">=", "<", ">" });
        }
    }
}