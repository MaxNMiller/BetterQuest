using NUnit.Framework;
using Spaa.Elements;
using Spaa.Habits;
using Spaa.Save;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialPlayVariantTests
    {
        private const string Today = "2026-09-27";

        private static SaveData FullRoster()
        {
            var data = new SaveData();
            HabitRoster.EnsureSeeded(data, new[]
            {
                HabitDefinition.CreateRuntime("sleep", "Sleep", Element.Rest, 10, 1),
                HabitDefinition.CreateRuntime("shower", "Shower", Element.SelfCare, 10, 1),
                HabitDefinition.CreateRuntime("eat", "Eat", Element.Food, 10, 1),
                HabitDefinition.CreateRuntime("journal", "Journal", Element.Rebuild, 10, 1),
                HabitDefinition.CreateRuntime("walk", "Walk", Element.Move, 10, 1)
            });
            return data;
        }

        [Test]
        public void FreshFullRoster_IsReady()
        {
            Assert.AreEqual(TutorialPlayVariant.Ready, TutorialPlayVariantResolver.Resolve(FullRoster(), Today));
        }

        [Test]
        public void BattleInProgress_IsReady()
        {
            var data = FullRoster();
            data.currentDayKey = Today;
            data.monsterSeed = 42;
            data.monsterHp = 30;

            Assert.AreEqual(TutorialPlayVariant.Ready, TutorialPlayVariantResolver.Resolve(data, Today));
        }

        [Test]
        public void MissingElement_IsMissingHabits()
        {
            var data = FullRoster();
            data.customHabits.RemoveAll(h => h.element == (int)Element.Move);

            Assert.AreEqual(TutorialPlayVariant.MissingHabits, TutorialPlayVariantResolver.Resolve(data, Today));
        }

        [Test]
        public void DefeatedToday_IsDefeated()
        {
            var data = FullRoster();
            data.currentDayKey = Today;
            data.monsterSeed = 42;
            data.monsterHp = 0;

            Assert.AreEqual(TutorialPlayVariant.Defeated, TutorialPlayVariantResolver.Resolve(data, Today));
        }

        [Test]
        public void DefeatedWithUnclaimedReward_IsLevelUpPending()
        {
            var data = FullRoster();
            data.currentDayKey = Today;
            data.monsterSeed = 42;
            data.monsterHp = 0;
            data.levelUpPending = true;

            Assert.AreEqual(TutorialPlayVariant.LevelUpPending, TutorialPlayVariantResolver.Resolve(data, Today));
        }
    }
}
