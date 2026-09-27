using NUnit.Framework;
using Spaa.Battle;

namespace Spaa.Tests.EditMode
{
    public class EndOfDayDamageCalculatorTests
    {
        [Test]
        public void Calculate_ReturnsTenPercentOfMaxHp()
        {
            Assert.AreEqual(10, EndOfDayDamageCalculator.Calculate(100));
        }

        [Test]
        public void Calculate_FloorsFraction()
        {
            Assert.AreEqual(9, EndOfDayDamageCalculator.Calculate(99));
        }

        [Test]
        public void Calculate_HasMinimumOfOne()
        {
            Assert.AreEqual(1, EndOfDayDamageCalculator.Calculate(1));
            Assert.AreEqual(1, EndOfDayDamageCalculator.Calculate(5));
        }
    }
}
