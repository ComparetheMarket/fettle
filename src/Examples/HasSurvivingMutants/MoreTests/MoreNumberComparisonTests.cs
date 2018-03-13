using HasSurvivingMutants.Implementation;
using NUnit.Framework;

namespace HasSurvivingMutants.MoreTests
{    
    public class MoreTests
    {
        [Test]
        public void DummyTest()
        {
            Assert.That(1+1, Is.EqualTo(2));
        }

        [Test]
        public void PostIncrement2()
        {
            // This test exists to show that coverage analysis handles methods
            // being covered by tests in multiple test projects.
            Assert.That(PartiallyTestedNumberComparison.Postincrement(5), Is.EqualTo(6));
        }
    }
}
