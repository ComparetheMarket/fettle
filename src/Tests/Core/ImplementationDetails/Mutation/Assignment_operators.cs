using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails.Mutation
{
    class Assignment_operators : Contexts.AssignmentExpressionMutators
    {
        [Test]
        public void Assignment_operators_are_swappable()
        {
            AssertAssignmentExpressionMutatesTo("+=", "-=", "*=", "/=", "%=");
            AssertAssignmentExpressionMutatesTo("-=", "*=", "/=", "%=", "+=");
            AssertAssignmentExpressionMutatesTo("*=", "/=", "%=", "+=", "-=");
            AssertAssignmentExpressionMutatesTo("/=", "%=", "+=", "-=", "*=");
            AssertAssignmentExpressionMutatesTo("%=", "+=", "-=", "*=", "/=");
        }
    }
}