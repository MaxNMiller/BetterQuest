using UnityEngine;
using Spaa.Elements;

namespace Spaa.Habits
{
    [CreateAssetMenu(menuName = "Spaa/Habits/Habit Definition", fileName = "New Habit Definition")]
    public class HabitDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Element element;
        [SerializeField] private int basePower = 10;
        [SerializeField] private int timesPerDay = 1;
        [SerializeField] private HabitInputType inputType = HabitInputType.Tap;

        public string Id => id;
        public string DisplayName => displayName;
        public Element Element => element;
        public int BasePower => basePower;
        public int TimesPerDay => timesPerDay;
        public HabitInputType InputType => inputType;

        public static HabitDefinition CreateRuntime(string id, string displayName, Element element, int basePower, int timesPerDay,
            HabitInputType inputType = HabitInputType.Tap)
        {
            var habit = CreateInstance<HabitDefinition>();
            habit.name = displayName;
            habit.id = id;
            habit.displayName = displayName;
            habit.element = element;
            habit.basePower = basePower;
            habit.timesPerDay = timesPerDay;
            habit.inputType = inputType;
            return habit;
        }
    }
}
