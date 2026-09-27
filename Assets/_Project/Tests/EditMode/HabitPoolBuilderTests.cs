using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Elements;
using Spaa.Habits;
using Spaa.Save;

namespace Spaa.Tests
{
    public class HabitPoolBuilderTests
    {
        private static CustomHabitData Custom(string id, string name, int element, int timesPerDay = 1, int basePower = 10)
        {
            return new CustomHabitData
            {
                id = id,
                displayName = name,
                element = element,
                timesPerDay = timesPerDay,
                basePower = basePower
            };
        }

        [Test]
        public void Build_AppendsCustomsAfterDefaults()
        {
            var defaultHabit = HabitDefinition.CreateRuntime("walk", "Walk", Element.Move, 10, 1);
            var customs = new List<CustomHabitData> { Custom("custom-1", "Yoga", (int)Element.Move, 2) };

            var pool = HabitPoolBuilder.Build(new[] { defaultHabit }, customs);

            Assert.AreEqual(2, pool.Count);
            Assert.AreSame(defaultHabit, pool[0]);
            Assert.AreEqual("custom-1", pool[1].Id);
            Assert.AreEqual("Yoga", pool[1].DisplayName);
            Assert.AreEqual(Element.Move, pool[1].Element);
            Assert.AreEqual(2, pool[1].TimesPerDay);
            Assert.AreEqual(HabitInputType.Tap, pool[1].InputType);
        }

        [Test]
        public void Build_SkipsInvalidCustomsAndNullDefaults()
        {
            var customs = new List<CustomHabitData>
            {
                null,
                Custom("custom-1", "  ", (int)Element.Rest),
                Custom("custom-2", "Bad element", 42),
                Custom("", "No id", (int)Element.Rest),
                Custom("custom-3", "Nap", (int)Element.Rest)
            };

            var pool = HabitPoolBuilder.Build(new HabitDefinition[] { null }, customs);

            Assert.AreEqual(1, pool.Count);
            Assert.AreEqual("Nap", pool[0].DisplayName);
        }

        [Test]
        public void Build_ClampsCorruptNumbers()
        {
            var customs = new List<CustomHabitData> { Custom("custom-1", "Odd", (int)Element.Food, timesPerDay: 0, basePower: 0) };

            var pool = HabitPoolBuilder.Build(null, customs);

            Assert.AreEqual(CustomHabitCatalog.MinTimesPerDay, pool[0].TimesPerDay);
            Assert.AreEqual(CustomHabitCatalog.DefaultBasePower, pool[0].BasePower);
        }

        [Test]
        public void CustomHabit_LandsInItsElementQueue()
        {
            var customs = new List<CustomHabitData> { Custom("custom-1", "Call mom", (int)Element.Rebuild, 2) };

            var queue = new HabitQueue(HabitPoolBuilder.Build(null, customs), seed: 7);

            Assert.AreEqual(2, queue.Remaining(Element.Rebuild));
            Assert.AreEqual("custom-1", queue.Next(Element.Rebuild).Definition.Id);
            Assert.AreEqual(0, queue.Remaining(Element.Move));
        }
    }
}
