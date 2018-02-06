using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    class Mathematical_operators : Contexts.BinaryExpressionMutators
    {
        [Test]
        public void Logical_operators_are_swappable()
        {
            AssertBinaryExpressionMutatesTo("+", new [] { "-", "*", "/", "%" });
            AssertBinaryExpressionMutatesTo("-", new [] { "*", "/", "%", "+" });
            AssertBinaryExpressionMutatesTo("*", new [] { "/", "%", "+", "-" });
            AssertBinaryExpressionMutatesTo("/", new [] { "%", "+", "-", "*" });
            AssertBinaryExpressionMutatesTo("%", new [] { "+", "-", "*", "/" });            
        }
    }
}