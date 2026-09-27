using NUnit.Framework;
using Spaa.Battle;

namespace Spaa.Tests
{
    public class DamageCalculatorTests
    {
        [Test]
        public void Neutral_Level1_NoWeakness_ReturnsBasePower()
        {
            int damage = DamageCalculator.Calculate(basePower: 10, elementLevel: 1, isWeakness: false);

            Assert.AreEqual(10, damage);
        }

        [Test]
        public void Weakness_DoublesDamage()
        {
            int damage = DamageCalculator.Calculate(basePower: 10, elementLevel: 1, isWeakness: true);

            Assert.AreEqual(20, damage);
        }

        [Test]
        public void LevelScaling_Level3_AppliesOneAndHalfMultiplier()
        {
            int damage = DamageCalculator.Calculate(basePower: 10, elementLevel: 3, isWeakness: false);

            Assert.AreEqual(15, damage);
        }

        [Test]
        public void LevelScaling_CombinedWithWeakness()
        {
            int damage = DamageCalculator.Calculate(basePower: 10, elementLevel: 2, isWeakness: true);

            Assert.AreEqual(25, damage);
        }

        [Test]
        public void Damage_NeverGoesBelowOne()
        {
            int damage = DamageCalculator.Calculate(basePower: 0, elementLevel: 1, isWeakness: false);

            Assert.AreEqual(1, damage);
        }
    }
}
