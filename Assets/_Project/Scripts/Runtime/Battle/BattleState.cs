using System;
using System.Collections.Generic;
using Spaa.Elements;
using Spaa.Habits;

namespace Spaa.Battle
{
    public class BattleState
    {
        private readonly List<BattleSlot> slots = new List<BattleSlot>(5);
        private readonly HabitQueue queue;
        private readonly IElementLevelProvider levelProvider;

        public MonsterData Monster { get; }
        public int MonsterHp { get; private set; }
        public int PlayerHp { get; private set; }
        public IReadOnlyList<BattleSlot> Slots => slots;
        public bool IsMonsterDefeated => MonsterHp <= 0;
        public bool TakeDamageEnabled { get; set; }

        public BattleState(MonsterData monster, HabitQueue queue, IElementLevelProvider levelProvider, int playerMaxHp, bool takeDamageEnabled = true)
        {
            Monster = monster;
            this.queue = queue;
            this.levelProvider = levelProvider;
            MonsterHp = monster.MaxHp;
            PlayerHp = playerMaxHp;
            TakeDamageEnabled = takeDamageEnabled;

            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                if (element == monster.Weakness)
                {
                    continue;
                }

                slots.Add(new BattleSlot(element, queue.Next(element)));
            }

            slots.Add(new BattleSlot(monster.Weakness, queue.Next(monster.Weakness)));
        }

        public BattleSlot GetSlot(int index)
        {
            return slots[index];
        }

        public int GetElementLevel(Element element)
        {
            return levelProvider.GetLevel(element);
        }

        public HabitInstance DrawNext(Element element)
        {
            return queue.Next(element);
        }

        public int RemainingFor(Element element)
        {
            return queue.Remaining(element);
        }

        public int ApplyDamageToMonster(int damage)
        {
            int before = MonsterHp;
            MonsterHp = Math.Max(0, MonsterHp - damage);
            return before - MonsterHp;
        }

        public void SetMonsterHp(int hp)
        {
            MonsterHp = Math.Max(0, hp);
        }

        public int ApplyDamageToPlayer(int damage)
        {
            int before = PlayerHp;
            PlayerHp = Math.Max(0, PlayerHp - damage);
            return before - PlayerHp;
        }

        public void SetPlayerHp(int hp)
        {
            PlayerHp = Math.Max(0, hp);
        }
    }
}
