using System;
using NUnit.Framework;
using Spaa.Battle;
using Spaa.Elements;

namespace Spaa.Tests
{
    public class DailyMonsterGeneratorTests
    {
        [Test]
        public void SameSeed_ProducesIdenticalMonster()
        {
            var a = DailyMonsterGenerator.Generate(seed: 555, totalExpectedNeutralDamage: 100);
            var b = DailyMonsterGenerator.Generate(seed: 555, totalExpectedNeutralDamage: 100);

            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Weakness, b.Weakness);
            Assert.AreEqual(a.MaxHp, b.MaxHp);
        }

        [Test]
        public void NewSeed_AvoidsPreviousWeakness()
        {
            int previousSeed = 555;
            var previousWeakness = DailyMonsterGenerator.Generate(previousSeed, 0).Weakness;

            for (int i = 0; i < 50; i++)
            {
                int seed = DailyMonsterGenerator.NewSeed(previousSeed);
                Assert.AreNotEqual(0, seed);
                Assert.AreNotEqual(previousWeakness, DailyMonsterGenerator.Generate(seed, 0).Weakness);
            }
        }

        [Test]
        public void MaxHp_IsCeilingOfSeventyPercentOfSum_ExactMultiple()
        {
            var monster = DailyMonsterGenerator.Generate(seed: 1, totalExpectedNeutralDamage: 100);

            Assert.AreEqual(70, monster.MaxHp);
        }

        [Test]
        public void MaxHp_IsCeilingOfSeventyPercentOfSum_RoundsUp()
        {
            var monster = DailyMonsterGenerator.Generate(seed: 1, totalExpectedNeutralDamage: 101);

            Assert.AreEqual(71, monster.MaxHp);
        }

        [Test]
        public void MaxHp_NeverBelowOne()
        {
            var monster = DailyMonsterGenerator.Generate(seed: 1, totalExpectedNeutralDamage: 0);

            Assert.AreEqual(1, monster.MaxHp);
        }

        [Test]
        public void Name_IsAdjectiveGloob_KeyedByWeakness()
        {
            var expected = new System.Collections.Generic.Dictionary<Element, string>
            {
                { Element.Rest, "Drowsy Gloob" },
                { Element.SelfCare, "Grubby Gloob" },
                { Element.Food, "Peckish Gloob" },
                { Element.Rebuild, "Lonely Gloob" },
                { Element.Move, "Sluggish Gloob" },
            };

            var seen = new System.Collections.Generic.HashSet<Element>();
            for (int seed = 1; seed < 200; seed++)
            {
                var monster = DailyMonsterGenerator.Generate(seed, totalExpectedNeutralDamage: 50);
                Assert.AreEqual(expected[monster.Weakness], monster.Name, $"seed {seed}");
                seen.Add(monster.Weakness);
            }

            Assert.AreEqual(5, seen.Count, "every weakness should appear across 200 seeds");
        }

        [Test]
        public void Weakness_IsAlwaysAValidElement()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var monster = DailyMonsterGenerator.Generate(seed, totalExpectedNeutralDamage: 50);

                Assert.IsTrue(Enum.IsDefined(typeof(Element), monster.Weakness));
            }
        }
    }
}
