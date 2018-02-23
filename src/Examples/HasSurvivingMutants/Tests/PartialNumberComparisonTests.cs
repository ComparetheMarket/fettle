using HasSurvivingMutants.Implementation;
using NUnit.Framework;

namespace HasSurvivingMutants.Tests
{
    // This fixture will (deliberately) produce surviving mutants because its tests are 
    // not comprehensive.
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

        [Test]
        public void Sum()
        {
            PartiallyTestedNumberComparison.Sum(1, 2);
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
    }
}
