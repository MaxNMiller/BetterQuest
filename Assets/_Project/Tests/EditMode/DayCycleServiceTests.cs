using NUnit.Framework;
using Spaa.DayCycle;

namespace Spaa.Tests
{
    public class DayCycleServiceTests
    {
        [Test]
        public void SameDay_ReturnsNoRollover()
        {
            var clock = new DebugDayClock("2026-09-26");
            var service = new DayCycleService(clock);

            var outcome = service.CheckRollover(lastDayKey: "2026-09-26", monsterDefeated: false);

            Assert.AreEqual(DayRolloverOutcome.NoRollover, outcome);
        }

        [Test]
        public void NextDay_MonsterAlive_ReturnsMonsterSurvived()
        {
            var clock = new DebugDayClock("2026-09-26");
            clock.AdvanceDay();
            var service = new DayCycleService(clock);

            var outcome = service.CheckRollover(lastDayKey: "2026-09-26", monsterDefeated: false);

            Assert.AreEqual(DayRolloverOutcome.NewDayMonsterSurvived, outcome);
        }

        [Test]
        public void NextDay_MonsterDefeated_ReturnsFreshDay()
        {
            var clock = new DebugDayClock("2026-09-26");
            clock.AdvanceDay();
            var service = new DayCycleService(clock);

            var outcome = service.CheckRollover(lastDayKey: "2026-09-26", monsterDefeated: true);

            Assert.AreEqual(DayRolloverOutcome.NewDayFresh, outcome);
        }

        [Test]
        public void AdvanceDay_MovesToNextCalendarDay()
        {
            var clock = new DebugDayClock("2026-09-26");

            clock.AdvanceDay();

            Assert.AreEqual("2026-09-27", clock.GetCurrentDayKey());
        }
    }
}
