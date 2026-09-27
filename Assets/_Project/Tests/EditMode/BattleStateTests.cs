using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Habits;

namespace Spaa.Tests
{
    public class BattleStateTests
    {
        private class FixedLevelProvider : IElementLevelProvider
        {
            private readonly int level;

            public FixedLevelProvider(int level)
            {
                this.level = level;
            }

            public int GetLevel(Element element) => level;
        }

        private HabitDefinition MakeHabit(string id, Element element)
        {
            var habit = ScriptableObject.CreateInstance<HabitDefinition>();
            var serialized = new UnityEditor.SerializedObject(habit);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = id;
            serialized.FindProperty("element").enumValueIndex = (int)element;
            serialized.FindProperty("basePower").intValue = 10;
            serialized.FindProperty("timesPerDay").intValue = 1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return habit;
        }

        private List<HabitDefinition> MakeFullPool()
        {
            var pool = new List<HabitDefinition>();
            foreach (Element element in System.Enum.GetValues(typeof(Element)))
            {
                pool.Add(MakeHabit(element + "_habit", element));
            }

            return pool;
        }

        private BattleState MakeState(Element weakness, int maxHp = 100)
        {
            var monster = new MonsterData("Test Monster", weakness, maxHp, seed: 1);
            var queue = new HabitQueue(MakeFullPool(), seed: 1);
            return new BattleState(monster, queue, new FixedLevelProvider(1), playerMaxHp: 100);
        }

        [Test]
        public void BattleState_HasFiveSlots_OnePerElement()
        {
            var state = MakeState(Element.Food);

            Assert.AreEqual(5, state.Slots.Count);
            var seen = new HashSet<Element>();
            foreach (var slot in state.Slots)
            {
                Assert.IsTrue(seen.Add(slot.Element), $"Duplicate slot for {slot.Element}");
            }
            Assert.AreEqual(5, seen.Count);
        }

        [Test]
        public void WeaknessElement_IsAlwaysAtIndexFour()
        {
            var state = MakeState(Element.Food);

            Assert.AreEqual(Element.Food, state.Slots[4].Element);
        }

        [Test]
        public void NonWeaknessSlots_KeepFixedEnumOrder()
        {
            var state = MakeState(Element.Food);

            Assert.AreEqual(Element.Rest, state.Slots[0].Element);
            Assert.AreEqual(Element.SelfCare, state.Slots[1].Element);
            Assert.AreEqual(Element.Rebuild, state.Slots[2].Element);
            Assert.AreEqual(Element.Move, state.Slots[3].Element);
        }

        [Test]
        public void ApplyDamageToMonster_ReducesHp()
        {
            var state = MakeState(Element.Food, maxHp: 100);

            state.ApplyDamageToMonster(30);

            Assert.AreEqual(70, state.MonsterHp);
        }

        [Test]
        public void ApplyDamageToMonster_ClampsAtZero()
        {
            var state = MakeState(Element.Food, maxHp: 10);

            state.ApplyDamageToMonster(999);

            Assert.AreEqual(0, state.MonsterHp);
        }

        [Test]
        public void IsMonsterDefeated_TrueWhenHpReachesZero()
        {
            var state = MakeState(Element.Food, maxHp: 10);

            Assert.IsFalse(state.IsMonsterDefeated);

            state.ApplyDamageToMonster(10);

            Assert.IsTrue(state.IsMonsterDefeated);
        }

        [Test]
        public void RemainingFor_CountsQueuedHabitsBehindTheSlot()
        {
            var pool = MakeFullPool();
            pool.Add(MakeHabit("rest_2", Element.Rest));
            pool.Add(MakeHabit("rest_3", Element.Rest));
            var monster = new MonsterData("Test Monster", Element.Food, 100, seed: 1);
            var state = new BattleState(monster, new HabitQueue(pool, seed: 1), new FixedLevelProvider(1), playerMaxHp: 100);

            Assert.AreEqual(2, state.RemainingFor(Element.Rest));
            Assert.AreEqual(0, state.RemainingFor(Element.Move));

            state.DrawNext(Element.Rest);
            Assert.AreEqual(1, state.RemainingFor(Element.Rest));
        }
    }
}
