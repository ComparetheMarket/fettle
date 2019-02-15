using HasSurvivingMutants.Implementation;
using Xunit;

namespace HasSurvivingMutants.XUnitTests
{
    public class PartialNumberComparisonTests
    {
        [Fact]
        public void IsPositiveReturnsTrueForPositiveNumbers()
        {
            Assert.True(PartiallyTestedNumberComparison.IsPositive(5));
        }
    }
}
