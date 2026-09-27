using System;

namespace Spaa.Battle
{
    public static class EndOfDayDamageCalculator
    {
        public static int Calculate(int monsterMaxHp)
        {
            return Math.Max(1, monsterMaxHp / 10);
        }
    }
}
