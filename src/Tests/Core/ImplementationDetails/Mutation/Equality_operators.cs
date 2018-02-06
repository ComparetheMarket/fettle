using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    class Equality_operators : Contexts.BinaryExpressionMutators
    {
        [Test]
        public void Equality_operators_are_swappable()
        {
            AssertBinaryExpressionMutatesTo("==", "!=");
            AssertBinaryExpressionMutatesTo("!=", "==");
        }
    }
}