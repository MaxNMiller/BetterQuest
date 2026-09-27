using System;
using Spaa.Elements;

namespace Spaa.Battle
{
    public static class DailyMonsterGenerator
    {
        private static readonly string[] NameAdjectives = { "Drowsy", "Grubby", "Peckish", "Lonely", "Sluggish" };

        public static int NewSeed(int previousSeed)
        {
            Element? previousWeakness = previousSeed == 0
                ? (Element?)null
                : Generate(previousSeed, 0).Weakness;

            int seed;
            do
            {
                seed = Guid.NewGuid().GetHashCode();
            }
            while (seed == 0 || (previousWeakness.HasValue &&
                   Generate(seed, 0).Weakness == previousWeakness.Value));

            return seed;
        }

        public static MonsterData Generate(int seed, double totalExpectedNeutralDamage)
        {
            var random = new Random(seed);
            var elements = (Element[])Enum.GetValues(typeof(Element));
            Element weakness = elements[random.Next(elements.Length)];

            int maxHp = Math.Max(1, (int)Math.Ceiling(0.7 * totalExpectedNeutralDamage));
            string name = NameAdjectives[(int)weakness] + " Gloob";

            return new MonsterData(name, weakness, maxHp, seed);
        }
    }
}
