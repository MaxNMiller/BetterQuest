using System;
using Spaa.Elements;
using Spaa.Save;

namespace Spaa.Habits
{
    public class CustomHabitCatalog
    {
        public const string IdPrefix = "custom-";
        public const int MaxNameLength = 24;
        public const int MinTimesPerDay = 1;
        public const int MaxTimesPerDay = 5;
        public const int DefaultBasePower = 10;

        private readonly SaveData _data;

        public CustomHabitCatalog(SaveData data)
        {
            _data = data;
        }

        public static bool IsCustomId(string id)
        {
            return id != null && id.StartsWith(IdPrefix, StringComparison.Ordinal);
        }

        public static string NormalizeName(string name)
        {
            if (name == null)
            {
                return string.Empty;
            }

            string trimmed = name.Trim();
            return trimmed.Length > MaxNameLength ? trimmed.Substring(0, MaxNameLength).TrimEnd() : trimmed;
        }

        public static int ClampTimesPerDay(int timesPerDay)
        {
            return Math.Max(MinTimesPerDay, Math.Min(MaxTimesPerDay, timesPerDay));
        }

        public CustomHabitData Find(string id)
        {
            foreach (var habit in _data.customHabits)
            {
                if (habit != null && habit.id == id)
                {
                    return habit;
                }
            }

            return null;
        }

        public bool TryAdd(string name, Element element, int timesPerDay, out CustomHabitData added)
        {
            added = null;
            string normalized = NormalizeName(name);
            if (normalized.Length == 0)
            {
                return false;
            }

            added = new CustomHabitData
            {
                id = IdPrefix + ReserveNextNumber(),
                displayName = normalized,
                element = (int)element,
                basePower = DefaultBasePower,
                timesPerDay = ClampTimesPerDay(timesPerDay)
            };
            _data.customHabits.Add(added);
            return true;
        }

        public bool TryUpdate(string id, string name, Element element, int timesPerDay)
        {
            var habit = Find(id);
            string normalized = NormalizeName(name);
            if (habit == null || normalized.Length == 0)
            {
                return false;
            }

            habit.displayName = normalized;
            habit.element = (int)element;
            habit.timesPerDay = ClampTimesPerDay(timesPerDay);
            return true;
        }

        public bool Remove(string id)
        {
            var habit = Find(id);
            return habit != null && _data.customHabits.Remove(habit);
        }

        private int ReserveNextNumber()
        {
            int next = Math.Max(1, _data.nextCustomHabitNumber);
            foreach (var habit in _data.customHabits)
            {
                if (habit != null && IsCustomId(habit.id)
                    && int.TryParse(habit.id.Substring(IdPrefix.Length), out int existing))
                {
                    next = Math.Max(next, existing + 1);
                }
            }

            _data.nextCustomHabitNumber = next + 1;
            return next;
        }
    }
}
