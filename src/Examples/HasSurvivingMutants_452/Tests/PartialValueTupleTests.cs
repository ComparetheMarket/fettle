using HasSurvivingMutants_452.Implementation;
using NUnit.Framework;

namespace HasSurvivingMutants_452.Tests
{
    public class PartialValueTupleTests
    {
        [Test]
        public void TupleExistsAndHasAValue()
        {
            // Tests will still pass when the implementation's "? x : y" is mutated to a "? y : x"
            Assert.That(PartiallyTestedValueTupleComparison.TupleExistsAndHasAValue((true, "wibble")).Item2, Is.EqualTo("wibble"));
        }
    }
}
