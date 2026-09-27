using System;
using System.Collections.Generic;
using Spaa.Elements;
using Spaa.Save;

namespace Spaa.UI
{
    public static class HabitJournalGrouping
    {
        public readonly struct ElementGroup
        {
            public Element Element { get; }
            public List<CustomHabitData> Habits { get; }

            public ElementGroup(Element element, List<CustomHabitData> habits)
            {
                Element = element;
                Habits = habits;
            }
        }

        public static List<ElementGroup> Group(IReadOnlyList<CustomHabitData> habits)
        {
            var elements = (Element[])Enum.GetValues(typeof(Element));
            var groups = new List<ElementGroup>(elements.Length);
            foreach (var element in elements)
            {
                groups.Add(new ElementGroup(element, new List<CustomHabitData>()));
            }

            if (habits == null)
            {
                return groups;
            }

            foreach (var habit in habits)
            {
                if (habit == null || habit.element < 0 || habit.element >= elements.Length)
                {
                    continue;
                }

                groups[habit.element].Habits.Add(habit);
            }

            return groups;
        }
    }
}
