using Spaa.Habits;

namespace Spaa.Battle
{
    public class HabitAttackCommand : IBattleCommand
    {
        private readonly int slotIndex;
        private HabitInstance consumedHabit;
        private int previousMonsterHp;

        public string Label => "Attack";

        public HabitAttackCommand(int slotIndex)
        {
            this.slotIndex = slotIndex;
        }

        public CommandResult Execute(BattleState state)
        {
            var slot = state.GetSlot(slotIndex);
            if (slot.IsDisabled)
            {
                return CommandResult.Failed("Slot is exhausted.");
            }

            consumedHabit = slot.CurrentHabit;
            previousMonsterHp = state.MonsterHp;

            bool isWeakness = slot.Element == state.Monster.Weakness;
            int level = state.GetElementLevel(slot.Element);
            int damage = DamageCalculator.Calculate(consumedHabit.Definition.BasePower, level, isWeakness);

            int applied = state.ApplyDamageToMonster(damage);

            var refillHabit = state.DrawNext(slot.Element);
            slot.Refill(refillHabit);

            bool defeated = state.IsMonsterDefeated;
            string message = isWeakness ? "Super effective!" : "Great job!";

            return new CommandResult(true, applied, isWeakness, defeated, message, slot.Element);
        }

        public void Undo(BattleState state)
        {
            var slot = state.GetSlot(slotIndex);
            state.SetMonsterHp(previousMonsterHp);
            slot.Refill(consumedHabit);
        }
    }
}
