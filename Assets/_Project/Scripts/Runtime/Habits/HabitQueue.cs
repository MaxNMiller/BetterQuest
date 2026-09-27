using System;
using System.Collections.Generic;
using Spaa.Elements;

namespace Spaa.Habits
{
    public class HabitQueue
    {
        private readonly Dictionary<Element, Queue<HabitInstance>> queuesByElement = new Dictionary<Element, Queue<HabitInstance>>();

        public HabitQueue(IReadOnlyList<HabitDefinition> pool, int seed)
        {
            var random = new Random(seed);

            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                var instances = new List<HabitInstance>();
                foreach (var habit in pool)
                {
                    if (habit == null || habit.Element != element)
                    {
                        continue;
                    }

                    int occurrences = Math.Max(1, habit.TimesPerDay);
                    for (int i = 0; i < occurrences; i++)
                    {
                        instances.Add(new HabitInstance(habit));
                    }
                }

                Shuffle(instances, random);
                queuesByElement[element] = new Queue<HabitInstance>(instances);
            }
        }

        public HabitInstance Next(Element element)
        {
            var queue = queuesByElement[element];
            return queue.Count > 0 ? queue.Dequeue() : null;
        }

        public int Remaining(Element element)
        {
            return queuesByElement[element].Count;
        }

        private static void Shuffle(IList<HabitInstance> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
