using HasSurvivingMutants.Implementation;
using NUnit.Framework;

namespace HasSurvivingMutants.Tests
{
    // This fixture will (deliberately) produce surviving mutants because its tests are 
    // not comprehensive. This is mostly achieved by the test methods calling the
    // implementation code but not having any assertions.
    public class PartialNumberComparisonTests
    {
        [Test]
        public void IsPositive()
        {
            Assert.That(PartiallyTestedNumberComparison.IsPositive(4), Is.True);
        }

        [Test]
        public void IsNegative()
        {
            Assert.That(PartiallyTestedNumberComparison.IsNegative(-4), Is.True);
        }

        [Test]
        public void IsZero()
        {            
            PartiallyTestedNumberComparison.IsZero(0);
        }

        [Test]
        public void AreBothZero()
        {            
            // Tests will still pass when the implementation's "&&" is mutated to a "||"
            Assert.That(PartiallyTestedNumberComparison.AreBothZero(0, 0), Is.True);
            Assert.That(PartiallyTestedNumberComparison.AreBothZero(1, 1), Is.False);
        }

        [TestCase(1,1)]
        [TestCase(2,2)]
        public void Sum(int a, int b)
        {
            PartiallyTestedNumberComparison.Sum(a,b);
        }

        [Test]
        public void Preincrement()
        {
            PartiallyTestedNumberComparison.Preincrement(5);
        }

        [Test]
        public void Postincrement()
        {
            PartiallyTestedNumberComparison.Postincrement(5);
        }

        [Test]
        public void PositiveOrNegative()
        {
            PartiallyTestedNumberComparison.PositiveOrNegative(1);
        }

        [Test]
        public void PositiveOrNegativeAsExpressionBody()
        {
            PartiallyTestedNumberComparison.PositiveOrNegativeAsExpressionBody(1);
        }

        [Test]
        public void Methods_with_ignored_statements()
        {
            PartiallyTestedNumberComparison.AddNumbers_should_be_ignored(5);
        }

        [Test, Ignore("deliberately ignored to check fettle ignores non-runnable tests")]
        public void ShouldNotBeRun()
        {
            Assert.That(PartiallyTestedNumberComparison.PositiveOrNegative(1), Is.EqualTo("positive"));
            Assert.That(PartiallyTestedNumberComparison.PositiveOrNegative(-1), Is.EqualTo("negative"));
        }

        [Test]
        public void EmptyMethod()
        {
            PartiallyTestedNumberComparison.EmptyMethod();
        }

        [Test]
        public void IntegerProperty()
        {
            var _ = PartiallyTestedNumberComparison.IntegerProperty;
        }

        [Test]
        public void BooleanToString()
        {
            PartiallyTestedNumberComparison.BooleanToString(true);
            PartiallyTestedNumberComparison.BooleanToString(false);
        }
    }
}
