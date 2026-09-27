using System;

namespace Spaa.Battle
{
    public static class DamageCalculator
    {
        public static int Calculate(int basePower, int elementLevel, bool isWeakness)
        {
            double levelMultiplier = 1.0 + 0.25 * (elementLevel - 1);
            double weaknessMultiplier = isWeakness ? 2.0 : 1.0;
            double raw = basePower * levelMultiplier * weaknessMultiplier;
            int rounded = (int)Math.Round(raw, MidpointRounding.AwayFromZero);
            return Math.Max(1, rounded);
        }
    }
}
