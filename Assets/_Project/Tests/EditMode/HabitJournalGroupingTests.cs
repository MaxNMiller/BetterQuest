using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Elements;
using Spaa.Save;
using Spaa.UI;

namespace Spaa.Tests
{
    public class HabitJournalGroupingTests
    {
        private static CustomHabitData Habit(string id, Element element)
        {
            return new CustomHabitData { id = id, displayName = id, element = (int)element, timesPerDay = 1 };
        }

        [Test]
        public void Groups_FollowElementEnumOrder_AndAllFiveArePresent()
        {
            var groups = HabitJournalGrouping.Group(new List<CustomHabitData> { Habit("walk", Element.Move), Habit("sleep", Element.Rest) });

            Assert.AreEqual(5, groups.Count);
            Assert.AreEqual(Element.Rest, groups[0].Element);
            Assert.AreEqual(Element.SelfCare, groups[1].Element);
            Assert.AreEqual(Element.Food, groups[2].Element);
            Assert.AreEqual(Element.Rebuild, groups[3].Element);
            Assert.AreEqual(Element.Move, groups[4].Element);
        }

        [Test]
        public void EmptyElements_HaveEmptyGroups()
        {
            var groups = HabitJournalGrouping.Group(new List<CustomHabitData> { Habit("sleep", Element.Rest) });

            Assert.AreEqual(1, groups[0].Habits.Count);
            Assert.AreEqual(0, groups[4].Habits.Count);
        }

        [Test]
        public void WithinGroup_SavedOrderIsKept()
        {
            var groups = HabitJournalGrouping.Group(new List<CustomHabitData>
            {
                Habit("b", Element.Food), Habit("x", Element.Rest), Habit("a", Element.Food), Habit("c", Element.Food)
            });

            CollectionAssert.AreEqual(new[] { "b", "a", "c" }, new[] { groups[2].Habits[0].id, groups[2].Habits[1].id, groups[2].Habits[2].id });
        }

        [Test]
        public void CorruptEntries_AreSkipped()
        {
            var groups = HabitJournalGrouping.Group(new List<CustomHabitData>
            {
                null, new CustomHabitData { id = "bad", element = 99 }, new CustomHabitData { id = "neg", element = -1 }, Habit("ok", Element.Move)
            });

            int total = 0;
            foreach (var group in groups)
            {
                total += group.Habits.Count;
            }

            Assert.AreEqual(1, total);
            Assert.AreEqual("ok", groups[4].Habits[0].id);
        }

        [Test]
        public void NullList_GivesFiveEmptyGroups()
        {
            var groups = HabitJournalGrouping.Group(null);

            Assert.AreEqual(5, groups.Count);
            Assert.AreEqual(0, groups[0].Habits.Count);
        }
    }
}
