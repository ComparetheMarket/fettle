using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    class Logical_operators : Contexts.BinaryExpressionMutators
    {
        [Test]
        public void Logical_operators_are_swappable()
        {
            AssertBinaryExpressionMutatesTo("&&", "||");
            AssertBinaryExpressionMutatesTo("||", "&&");
        }
    }
}