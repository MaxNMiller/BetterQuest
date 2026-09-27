using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Elements;
using Spaa.Habits;
using Spaa.Save;

namespace Spaa.Tests
{
    public class HabitRosterTests
    {
        private static HabitDefinition[] OnePerElement()
        {
            return new[]
            {
                HabitDefinition.CreateRuntime("sleep", "Sleep", Element.Rest, 10, 1, HabitInputType.Number),
                HabitDefinition.CreateRuntime("shower", "Shower", Element.SelfCare, 10, 1),
                HabitDefinition.CreateRuntime("hydrate", "Hydrate", Element.Food, 10, 3),
                HabitDefinition.CreateRuntime("journal", "Journal", Element.Rebuild, 10, 1),
                HabitDefinition.CreateRuntime("walk", "Walk", Element.Move, 10, 1)
            };
        }

        [Test]
        public void EnsureSeeded_CopiesDefaultsWithTheirIdsAndFields()
        {
            var data = new SaveData();

            bool changed = HabitRoster.EnsureSeeded(data, OnePerElement());

            Assert.IsTrue(changed);
            Assert.IsTrue(data.habitsSeeded);
            Assert.AreEqual(5, data.customHabits.Count);
            var hydrate = data.customHabits[2];
            Assert.AreEqual("hydrate", hydrate.id);
            Assert.AreEqual("Hydrate", hydrate.displayName);
            Assert.AreEqual((int)Element.Food, hydrate.element);
            Assert.AreEqual(3, hydrate.timesPerDay);
            Assert.AreEqual((int)HabitInputType.Number, data.customHabits[0].inputType);
        }

        [Test]
        public void EnsureSeeded_RunsOnlyOnce_SoDeletedDefaultsStayDeleted()
        {
            var data = new SaveData();
            HabitRoster.EnsureSeeded(data, OnePerElement());
            new CustomHabitCatalog(data).Remove("walk");

            bool changed = HabitRoster.EnsureSeeded(data, OnePerElement());

            Assert.IsFalse(changed);
            Assert.AreEqual(4, data.customHabits.Count);
            Assert.IsNull(new CustomHabitCatalog(data).Find("walk"));
        }

        [Test]
        public void EnsureSeeded_KeepsExistingPlayerHabitsAfterDefaults()
        {
            var data = new SaveData();
            new CustomHabitCatalog(data).TryAdd("Yoga", Element.Move, 1, out var yoga);

            HabitRoster.EnsureSeeded(data, OnePerElement());

            Assert.AreEqual(6, data.customHabits.Count);
            Assert.AreEqual(yoga.id, data.customHabits[5].id);
        }

        [Test]
        public void DefaultHabits_CanBeEditedAfterSeeding()
        {
            var data = new SaveData();
            HabitRoster.EnsureSeeded(data, OnePerElement());

            bool updated = new CustomHabitCatalog(data).TryUpdate("walk", "Run", Element.Move, 2);

            Assert.IsTrue(updated);
            Assert.AreEqual("Run", new CustomHabitCatalog(data).Find("walk").displayName);
        }

        [Test]
        public void EnsureSeeded_WithNoDefaults_DoesNotMarkSeeded()
        {
            var data = new SaveData();

            Assert.IsFalse(HabitRoster.EnsureSeeded(data, null));
            Assert.IsFalse(HabitRoster.EnsureSeeded(data, new HabitDefinition[] { null }));
            Assert.IsFalse(data.habitsSeeded);

            Assert.IsTrue(HabitRoster.EnsureSeeded(data, OnePerElement()));
            Assert.AreEqual(5, data.customHabits.Count);
        }

        [Test]
        public void MissingElements_ListsElementsWithNoHabit()
        {
            var data = new SaveData();
            HabitRoster.EnsureSeeded(data, OnePerElement());
            Assert.IsTrue(HabitRoster.CanPlay(data.customHabits));

            var catalog = new CustomHabitCatalog(data);
            catalog.Remove("sleep");
            catalog.TryUpdate("walk", "Walk", Element.Food, 1);

            var missing = HabitRoster.MissingElements(data.customHabits);
            CollectionAssert.AreEqual(new[] { Element.Rest, Element.Move }, missing);
            Assert.IsFalse(HabitRoster.CanPlay(data.customHabits));
        }

        [Test]
        public void MissingElements_EmptyOrNullRoster_BlocksPlay()
        {
            Assert.AreEqual(5, HabitRoster.MissingElements(new List<CustomHabitData>()).Count);
            Assert.AreEqual(5, HabitRoster.MissingElements(null).Count);
            Assert.IsFalse(HabitRoster.CanPlay(null));
        }
    }
}
