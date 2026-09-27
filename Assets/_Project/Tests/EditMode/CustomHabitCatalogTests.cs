using System.IO;
using NUnit.Framework;
using Spaa.Elements;
using Spaa.Habits;
using Spaa.Save;

namespace Spaa.Tests
{
    public class CustomHabitCatalogTests
    {
        [Test]
        public void TryAdd_TrimsNameAndAssignsDefaults()
        {
            var data = new SaveData();
            var catalog = new CustomHabitCatalog(data);

            bool added = catalog.TryAdd("  Read a book  ", Element.Rest, 2, out var habit);

            Assert.IsTrue(added);
            Assert.AreEqual("Read a book", habit.displayName);
            Assert.AreEqual((int)Element.Rest, habit.element);
            Assert.AreEqual(2, habit.timesPerDay);
            Assert.AreEqual(CustomHabitCatalog.DefaultBasePower, habit.basePower);
            Assert.IsTrue(CustomHabitCatalog.IsCustomId(habit.id));
            Assert.AreEqual(1, data.customHabits.Count);
        }

        [Test]
        public void TryAdd_RejectsBlankName()
        {
            var data = new SaveData();
            var catalog = new CustomHabitCatalog(data);

            Assert.IsFalse(catalog.TryAdd("   ", Element.Move, 1, out _));
            Assert.IsFalse(catalog.TryAdd(null, Element.Move, 1, out _));
            Assert.AreEqual(0, data.customHabits.Count);
        }

        [Test]
        public void TryAdd_ClampsTimesPerDayAndTruncatesName()
        {
            var catalog = new CustomHabitCatalog(new SaveData());

            catalog.TryAdd(new string('x', 40), Element.Food, 99, out var high);
            catalog.TryAdd("Low", Element.Food, 0, out var low);

            Assert.AreEqual(CustomHabitCatalog.MaxNameLength, high.displayName.Length);
            Assert.AreEqual(CustomHabitCatalog.MaxTimesPerDay, high.timesPerDay);
            Assert.AreEqual(CustomHabitCatalog.MinTimesPerDay, low.timesPerDay);
        }

        [Test]
        public void Ids_AreUniqueAndNeverReusedAfterRemove()
        {
            var catalog = new CustomHabitCatalog(new SaveData());

            catalog.TryAdd("A", Element.Move, 1, out var a);
            catalog.TryAdd("B", Element.Move, 1, out var b);
            Assert.AreNotEqual(a.id, b.id);

            Assert.IsTrue(catalog.Remove(b.id));
            catalog.TryAdd("C", Element.Move, 1, out var c);

            Assert.AreNotEqual(b.id, c.id);
            Assert.AreNotEqual(a.id, c.id);
        }

        [Test]
        public void TryUpdate_ChangesFieldsButKeepsId()
        {
            var catalog = new CustomHabitCatalog(new SaveData());
            catalog.TryAdd("Walk dog", Element.Move, 1, out var habit);
            string id = habit.id;

            bool updated = catalog.TryUpdate(id, " Feed dog ", Element.Food, 3);

            Assert.IsTrue(updated);
            var found = catalog.Find(id);
            Assert.AreEqual("Feed dog", found.displayName);
            Assert.AreEqual((int)Element.Food, found.element);
            Assert.AreEqual(3, found.timesPerDay);
        }

        [Test]
        public void TryUpdate_RejectsUnknownIdOrBlankName()
        {
            var catalog = new CustomHabitCatalog(new SaveData());
            catalog.TryAdd("Walk dog", Element.Move, 1, out var habit);

            Assert.IsFalse(catalog.TryUpdate("custom-999", "X", Element.Move, 1));
            Assert.IsFalse(catalog.TryUpdate(habit.id, "  ", Element.Move, 1));
            Assert.AreEqual("Walk dog", catalog.Find(habit.id).displayName);
        }

        [Test]
        public void Remove_UnknownId_ReturnsFalse()
        {
            var catalog = new CustomHabitCatalog(new SaveData());

            Assert.IsFalse(catalog.Remove("custom-1"));
        }

        [Test]
        public void CustomHabits_RoundTripThroughJsonSaveService()
        {
            string path = Path.Combine(Path.GetTempPath(), $"spaa_custom_{System.Guid.NewGuid():N}.json");
            try
            {
                var data = new SaveData();
                var catalog = new CustomHabitCatalog(data);
                catalog.TryAdd("Stretch hips", Element.Move, 2, out var habit);
                var service = new JsonSaveService(path);
                service.Save(data);

                var loaded = service.Load();
                var reloadedCatalog = new CustomHabitCatalog(loaded);
                var found = reloadedCatalog.Find(habit.id);

                Assert.IsNotNull(found);
                Assert.AreEqual("Stretch hips", found.displayName);
                Assert.AreEqual(2, found.timesPerDay);

                reloadedCatalog.TryAdd("Another", Element.Move, 1, out var next);
                Assert.AreNotEqual(habit.id, next.id);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
