using System;
using System.Collections.Generic;
using Spaa.Elements;
using Spaa.Save;

namespace Spaa.Habits
{
    public static class HabitRoster
    {
        public static bool EnsureSeeded(SaveData data, IReadOnlyList<HabitDefinition> defaults)
        {
            if (data.habitsSeeded || !HasAnyDefault(defaults))
            {
                return false;
            }

            var seeded = new List<CustomHabitData>();
            foreach (var habit in defaults)
            {
                if (habit == null || string.IsNullOrEmpty(habit.Id) || ContainsId(data.customHabits, habit.Id))
                {
                    continue;
                }

                seeded.Add(new CustomHabitData
                {
                    id = habit.Id,
                    displayName = habit.DisplayName,
                    element = (int)habit.Element,
                    basePower = habit.BasePower,
                    timesPerDay = habit.TimesPerDay,
                    inputType = (int)habit.InputType
                });
            }

            data.customHabits.InsertRange(0, seeded);
            data.habitsSeeded = true;
            return true;
        }

        private static bool HasAnyDefault(IReadOnlyList<HabitDefinition> defaults)
        {
            if (defaults == null)
            {
                return false;
            }

            foreach (var habit in defaults)
            {
                if (habit != null && !string.IsNullOrEmpty(habit.Id))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Element> MissingElements(IReadOnlyList<CustomHabitData> habits)
        {
            var missing = new List<Element>();
            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                if (!HasElement(habits, element))
                {
                    missing.Add(element);
                }
            }

            return missing;
        }

        public static bool CanPlay(IReadOnlyList<CustomHabitData> habits)
        {
            return MissingElements(habits).Count == 0;
        }

        private static bool HasElement(IReadOnlyList<CustomHabitData> habits, Element element)
        {
            if (habits == null)
            {
                return false;
            }

            foreach (var habit in habits)
            {
                if (habit != null && habit.element == (int)element
                    && CustomHabitCatalog.NormalizeName(habit.displayName).Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsId(List<CustomHabitData> habits, string id)
        {
            foreach (var habit in habits)
            {
                if (habit != null && habit.id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
