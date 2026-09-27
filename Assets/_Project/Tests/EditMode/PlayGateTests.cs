using NUnit.Framework;
using Spaa.DayCycle;
using Spaa.Elements;
using Spaa.Flow;
using Spaa.Habits;
using Spaa.Save;

namespace Spaa.Tests
{
    public class PlayGateTests
    {
        private const string Today = "2026-09-26";

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

        private static SaveData DefeatedToday()
        {
            var data = FullRoster();
            data.currentDayKey = Today;
            data.monsterSeed = 42;
            data.monsterHp = 0;
            return data;
        }

        [Test]
        public void FreshSave_CanPlay()
        {
            Assert.AreEqual(PlayBlockReason.None, PlayGate.Evaluate(FullRoster(), Today));
        }

        [Test]
        public void MonsterAliveToday_CanPlay()
        {
            var data = DefeatedToday();
            data.monsterHp = 5;

            Assert.AreEqual(PlayBlockReason.None, PlayGate.Evaluate(data, Today));
        }

        [Test]
        public void MonsterDefeatedToday_BlocksPlay()
        {
            Assert.AreEqual(PlayBlockReason.MonsterDefeatedToday, PlayGate.Evaluate(DefeatedToday(), Today));
        }

        [Test]
        public void MonsterDefeatedToday_WithUnclaimedLevelUp_CanPlay()
        {
            var data = DefeatedToday();
            data.levelUpPending = true;

            Assert.AreEqual(PlayBlockReason.None, PlayGate.Evaluate(data, Today));
        }

        [Test]
        public void MonsterDefeatedOnAnEarlierDay_CanPlay()
        {
            Assert.AreEqual(PlayBlockReason.None, PlayGate.Evaluate(DefeatedToday(), "2026-09-27"));
        }

        [Test]
        public void MissingHabits_BlocksPlay()
        {
            var data = FullRoster();
            new CustomHabitCatalog(data).Remove("walk");

            Assert.AreEqual(PlayBlockReason.MissingHabits, PlayGate.Evaluate(data, Today));
        }

        [Test]
        public void StartFreshDay_UnblocksDefeatedToday_AndKeepsProgress()
        {
            var data = DefeatedToday();
            data.elementLevels[2] = 3;
            data.playerHp = 70;
            data.completedHabitIds.Add("walk");

            DebugDayActions.StartFreshDay(data);

            Assert.AreEqual(PlayBlockReason.None, PlayGate.Evaluate(data, Today));
            Assert.AreEqual(string.Empty, data.currentDayKey);
            Assert.AreEqual(0, data.monsterSeed);
            Assert.AreEqual(42, data.lastMonsterSeed);
            Assert.IsFalse(data.levelUpPending);
            Assert.AreEqual(0, data.completedHabitIds.Count);
            Assert.AreEqual(3, data.elementLevels[2]);
            Assert.AreEqual(70, data.playerHp);
            Assert.AreEqual(5, data.customHabits.Count);
        }
    }
}
