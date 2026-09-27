using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Habits;

namespace Spaa.Tests
{
    public class MonsterEndOfDayAttackCommandTests
    {
        private class FixedLevelProvider : IElementLevelProvider
        {
            public int GetLevel(Element element) => 1;
        }

        private BattleState MakeState(int playerMaxHp, bool takeDamageEnabled)
        {
            var monster = new MonsterData("Test Monster", Element.Food, maxHp: 100, seed: 1);
            var queue = new HabitQueue(new List<HabitDefinition>(), seed: 1);
            return new BattleState(monster, queue, new FixedLevelProvider(), playerMaxHp, takeDamageEnabled);
        }

        [Test]
        public void Execute_TakeDamageOn_DamagesPlayer()
        {
            var state = MakeState(playerMaxHp: 100, takeDamageEnabled: true);
            var invoker = new BattleCommandInvoker(state);

            var result = invoker.ExecuteCommand(new MonsterEndOfDayAttackCommand(15));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(15, result.DamageDealt);
            Assert.AreEqual(85, state.PlayerHp);
        }

        [Test]
        public void Execute_TakeDamageOff_DealsNoDamage()
        {
            var state = MakeState(playerMaxHp: 100, takeDamageEnabled: false);
            var invoker = new BattleCommandInvoker(state);

            var result = invoker.ExecuteCommand(new MonsterEndOfDayAttackCommand(15));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.DamageDealt);
            Assert.AreEqual(100, state.PlayerHp);
        }

        [Test]
        public void Execute_PlayerHp_ClampsAtZero()
        {
            var state = MakeState(playerMaxHp: 10, takeDamageEnabled: true);
            var invoker = new BattleCommandInvoker(state);

            var result = invoker.ExecuteCommand(new MonsterEndOfDayAttackCommand(999));

            Assert.AreEqual(10, result.DamageDealt);
            Assert.AreEqual(0, state.PlayerHp);
        }
    }
}
