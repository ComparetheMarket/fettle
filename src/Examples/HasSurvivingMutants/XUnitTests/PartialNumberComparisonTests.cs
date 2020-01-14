using HasSurvivingMutants.MoreImplementation;
using Xunit;

namespace HasSurvivingMutants.XUnitTests
{
    public class PartialNumberComparisonTests
    {
        [Fact]
        public void IsMoreThanOneHundredReturnsTrueForNumbersGreaterThanOneHundred()
        {
            Assert.True(MorePartiallyTestedNumberComparison.IsMoreThanOneHundred(101));
        }
    }
}