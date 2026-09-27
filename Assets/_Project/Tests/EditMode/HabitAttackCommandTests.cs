using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Habits;

namespace Spaa.Tests
{
    public class HabitAttackCommandTests
    {
        private class FixedLevelProvider : IElementLevelProvider
        {
            public int GetLevel(Element element) => 1;
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

        private List<HabitDefinition> MakeFullPool(int perElement = 2)
        {
            var pool = new List<HabitDefinition>();
            foreach (Element element in System.Enum.GetValues(typeof(Element)))
            {
                for (int i = 0; i < perElement; i++)
                {
                    pool.Add(MakeHabit(element + "_habit_" + i, element));
                }
            }

            return pool;
        }

        private BattleState MakeState(Element weakness, int maxHp = 1000)
        {
            var monster = new MonsterData("Test Monster", weakness, maxHp, seed: 1);
            var queue = new HabitQueue(MakeFullPool(), seed: 1);
            return new BattleState(monster, queue, new FixedLevelProvider(), playerMaxHp: 100);
        }

        [Test]
        public void Execute_OnWeaknessSlot_DealsDoubleDamage()
        {
            var state = MakeState(Element.Food);
            var invoker = new BattleCommandInvoker(state);
            int hpBefore = state.MonsterHp;

            var result = invoker.ExecuteCommand(new HabitAttackCommand(4));

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.WasWeakness);
            Assert.AreEqual(20, result.DamageDealt);
            Assert.AreEqual(hpBefore - 20, state.MonsterHp);
        }

        [Test]
        public void Execute_RefillsSlotWithNextHabitOfSameElement()
        {
            var state = MakeState(Element.Food);
            var invoker = new BattleCommandInvoker(state);
            var slot = state.GetSlot(4);
            var firstHabitId = slot.CurrentHabit.Definition.Id;

            invoker.ExecuteCommand(new HabitAttackCommand(4));

            Assert.IsFalse(slot.IsDisabled);
            Assert.AreEqual(Element.Food, slot.CurrentHabit.Definition.Element);
            Assert.AreNotEqual(firstHabitId, slot.CurrentHabit.Definition.Id);
        }

        [Test]
        public void Execute_ExhaustsElementQueue_DisablesSlot()
        {
            var state = MakeState(Element.Food);
            var invoker = new BattleCommandInvoker(state);

            invoker.ExecuteCommand(new HabitAttackCommand(4));
            var secondResult = invoker.ExecuteCommand(new HabitAttackCommand(4));
            var slot = state.GetSlot(4);

            Assert.IsTrue(secondResult.Success);
            Assert.IsTrue(slot.IsDisabled);

            var thirdResult = invoker.ExecuteCommand(new HabitAttackCommand(4));
            Assert.IsFalse(thirdResult.Success);
        }

        [Test]
        public void UndoLast_RestoresHpAndSlotHabit()
        {
            var state = MakeState(Element.Food);
            var invoker = new BattleCommandInvoker(state);
            var slot = state.GetSlot(4);
            var originalHabitId = slot.CurrentHabit.Definition.Id;
            int hpBefore = state.MonsterHp;

            invoker.ExecuteCommand(new HabitAttackCommand(4));
            Assert.AreNotEqual(hpBefore, state.MonsterHp);

            bool undone = invoker.UndoLast();

            Assert.IsTrue(undone);
            Assert.AreEqual(hpBefore, state.MonsterHp);
            Assert.AreEqual(originalHabitId, slot.CurrentHabit.Definition.Id);
        }

        [Test]
        public void Execute_ResultCarriesAttackingSlotElement()
        {
            var state = MakeState(Element.Rest);
            var invoker = new BattleCommandInvoker(state);

            var neutral = invoker.ExecuteCommand(new HabitAttackCommand(0));
            var weak = invoker.ExecuteCommand(new HabitAttackCommand(4));

            Assert.AreEqual(Element.SelfCare, neutral.Element);
            Assert.AreEqual(Element.Rest, weak.Element);
        }
    }
}
