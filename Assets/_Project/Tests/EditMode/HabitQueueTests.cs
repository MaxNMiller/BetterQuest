using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Spaa.Elements;
using Spaa.Habits;

namespace Spaa.Tests
{
    public class HabitQueueTests
    {
        private HabitDefinition MakeHabit(string id, Element element, int timesPerDay = 1)
        {
            var habit = ScriptableObject.CreateInstance<HabitDefinition>();
            var serialized = new UnityEditor.SerializedObject(habit);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = id;
            serialized.FindProperty("element").enumValueIndex = (int)element;
            serialized.FindProperty("basePower").intValue = 10;
            serialized.FindProperty("timesPerDay").intValue = timesPerDay;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return habit;
        }

        [Test]
        public void SameSeed_ProducesSameOrder()
        {
            var pool = new List<HabitDefinition>
            {
                MakeHabit("a", Element.Move),
                MakeHabit("b", Element.Move),
                MakeHabit("c", Element.Move),
                MakeHabit("d", Element.Move),
            };

            var queueA = new HabitQueue(pool, seed: 12345);
            var queueB = new HabitQueue(pool, seed: 12345);

            for (int i = 0; i < pool.Count; i++)
            {
                var a = queueA.Next(Element.Move);
                var b = queueB.Next(Element.Move);
                Assert.AreEqual(a.Definition.Id, b.Definition.Id);
            }
        }

        [Test]
        public void TimesPerDay_ExpandsIntoMultipleInstances()
        {
            var pool = new List<HabitDefinition>
            {
                MakeHabit("hydrate", Element.Food, timesPerDay: 3),
            };

            var queue = new HabitQueue(pool, seed: 1);

            Assert.AreEqual(3, queue.Remaining(Element.Food));
        }

        [Test]
        public void Habits_LandInCorrectElementQueue()
        {
            var pool = new List<HabitDefinition>
            {
                MakeHabit("sleep", Element.Rest),
                MakeHabit("walk", Element.Move),
            };

            var queue = new HabitQueue(pool, seed: 1);

            Assert.AreEqual(1, queue.Remaining(Element.Rest));
            Assert.AreEqual(1, queue.Remaining(Element.Move));
            Assert.AreEqual(0, queue.Remaining(Element.Food));
            Assert.AreEqual(0, queue.Remaining(Element.SelfCare));
            Assert.AreEqual(0, queue.Remaining(Element.Rebuild));

            Assert.AreEqual("sleep", queue.Next(Element.Rest).Definition.Id);
            Assert.AreEqual("walk", queue.Next(Element.Move).Definition.Id);
        }

        [Test]
        public void EmptyPool_IsSafe()
        {
            var pool = new List<HabitDefinition>();

            var queue = new HabitQueue(pool, seed: 1);

            Assert.AreEqual(0, queue.Remaining(Element.Rest));
            Assert.IsNull(queue.Next(Element.Rest));
        }
    }
}
