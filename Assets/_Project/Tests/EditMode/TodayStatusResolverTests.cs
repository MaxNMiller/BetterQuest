using NUnit.Framework;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Flow;
using Spaa.Habits;
using Spaa.Save;

namespace Spaa.Tests
{
    public class TodayStatusResolverTests
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

        private static SaveData StartedToday(int hp)
        {
            var data = FullRoster();
            data.currentDayKey = Today;
            data.monsterSeed = 4242;
            data.monsterHp = hp;
            return data;
        }

        [Test]
        public void NoBattleYet_IsFresh_WithoutSpoilers()
        {
            var status = TodayStatusResolver.Resolve(FullRoster(), Today);

            Assert.AreEqual(TodayStatusKind.Fresh, status.Kind);
            Assert.IsFalse(status.Weakness.HasValue);
        }

        [Test]
        public void MonsterAliveToday_IsInProgress_WithSavedMonstersWeaknessAndName()
        {
            var status = TodayStatusResolver.Resolve(StartedToday(12), Today);
            var expected = DailyMonsterGenerator.Generate(4242, 0);

            Assert.AreEqual(TodayStatusKind.InProgress, status.Kind);
            Assert.AreEqual(expected.Weakness, status.Weakness);
            Assert.AreEqual(expected.Name, status.MonsterName);
        }

        [Test]
        public void DefeatedToday_IsDefeated()
        {
            var status = TodayStatusResolver.Resolve(StartedToday(0), Today);

            Assert.AreEqual(TodayStatusKind.Defeated, status.Kind);
        }

        [Test]
        public void DefeatedWithUnclaimedLevelUp_IsLevelUpPending()
        {
            var data = StartedToday(0);
            data.levelUpPending = true;

            var status = TodayStatusResolver.Resolve(data, Today);

            Assert.AreEqual(TodayStatusKind.LevelUpPending, status.Kind);
            Assert.IsTrue(status.Weakness.HasValue);
        }

        [Test]
        public void MissingElementHabits_IsMissingHabits_ListingThem()
        {
            var data = FullRoster();
            data.customHabits.RemoveAll(h => h.element == (int)Element.Move);

            var status = TodayStatusResolver.Resolve(data, Today);

            Assert.AreEqual(TodayStatusKind.MissingHabits, status.Kind);
            CollectionAssert.AreEqual(new[] { Element.Move }, status.MissingElements);
        }

        [Test]
        public void StaleDayKey_IsFresh()
        {
            var data = StartedToday(12);
            data.currentDayKey = "2026-09-25";

            Assert.AreEqual(TodayStatusKind.Fresh, TodayStatusResolver.Resolve(data, Today).Kind);
        }

        [Test]
        public void ZeroSeedForToday_IsFresh()
        {
            var data = StartedToday(12);
            data.monsterSeed = 0;

            Assert.AreEqual(TodayStatusKind.Fresh, TodayStatusResolver.Resolve(data, Today).Kind);
        }
    }
}
