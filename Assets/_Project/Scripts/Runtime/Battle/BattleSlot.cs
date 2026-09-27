using Spaa.Elements;
using Spaa.Habits;

namespace Spaa.Battle
{
    public class BattleSlot
    {
        public Element Element { get; }
        public HabitInstance CurrentHabit { get; private set; }
        public bool IsDisabled => CurrentHabit == null;

        public BattleSlot(Element element, HabitInstance initialHabit)
        {
            Element = element;
            CurrentHabit = initialHabit;
        }

        public void Refill(HabitInstance next)
        {
            CurrentHabit = next;
        }
    }
}
