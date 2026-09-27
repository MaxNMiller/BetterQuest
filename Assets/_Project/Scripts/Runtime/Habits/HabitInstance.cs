namespace Spaa.Habits
{
    public class HabitInstance
    {
        public HabitDefinition Definition { get; }

        public HabitInstance(HabitDefinition definition)
        {
            Definition = definition;
        }
    }
}
