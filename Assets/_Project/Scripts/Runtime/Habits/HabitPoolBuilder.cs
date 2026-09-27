using System;
using System.Collections.Generic;
using Spaa.Elements;
using Spaa.Save;

namespace Spaa.Habits
{
    public static class HabitPoolBuilder
    {
        public static List<HabitDefinition> Build(IReadOnlyList<HabitDefinition> defaults, IReadOnlyList<CustomHabitData> customs)
        {
            var pool = new List<HabitDefinition>();

            if (defaults != null)
            {
                foreach (var habit in defaults)
                {
                    if (habit != null)
                    {
                        pool.Add(habit);
                    }
                }
            }

            if (customs == null)
            {
                return pool;
            }

            int elementCount = Enum.GetValues(typeof(Element)).Length;
            foreach (var custom in customs)
            {
                if (custom == null || string.IsNullOrEmpty(custom.id)
                    || custom.element < 0 || custom.element >= elementCount)
                {
                    continue;
                }

                string name = CustomHabitCatalog.NormalizeName(custom.displayName);
                if (name.Length == 0)
                {
                    continue;
                }

                int basePower = custom.basePower > 0 ? custom.basePower : CustomHabitCatalog.DefaultBasePower;
                var inputType = Enum.IsDefined(typeof(HabitInputType), custom.inputType)
                    ? (HabitInputType)custom.inputType
                    : HabitInputType.Tap;
                pool.Add(HabitDefinition.CreateRuntime(
                    custom.id,
                    name,
                    (Element)custom.element,
                    basePower,
                    CustomHabitCatalog.ClampTimesPerDay(custom.timesPerDay),
                    inputType));
            }

            return pool;
        }
    }
}
