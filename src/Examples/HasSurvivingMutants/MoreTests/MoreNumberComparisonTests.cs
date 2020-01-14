using System;
using System.Collections.Generic;
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

        [TestCase(1, 2)]
        [TestCase(4, 5)]
        public void PreIncrement2(int num, int expected)
        {
            // This test exists to show that coverage analysis handles methods
            // being covered by parameterised tests.
            Assert.That(PartiallyTestedNumberComparison.Preincrement(num), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Sum2TestData
        {
            get
            {
                yield return new TestCaseData(1, 1, 2);
                yield return new TestCaseData(-1, 2, 1);
            }
        }

        [TestCaseSource(nameof(Sum2TestData))]
        public void Sum2(int a, int b, int expectedSum)
        {
            // This test exists to show that coverage analysis handles methods
            // being covered by tests that use a test case source.
            Assert.That(PartiallyTestedNumberComparison.Sum(a, b), Is.EqualTo(expectedSum));
        }

        [Test]
        public void ThrowingMethod()
        {
            // This test exists to show that coverage analysis handles methods
            // that throw exceptions being covered by tests.
            Assert.Throws<InvalidOperationException>(OtherMethods.ThrowingMethod);
        }
    }
}
