namespace Spaa.Battle
{
    public static class MonsterExpressionRules
    {
        public const float DefaultLowHpFraction = 0.3f;

        public static MonsterExpression Resolve(int hp, int maxHp, bool isHurt, float lowHpFraction = DefaultLowHpFraction)
        {
            if (hp <= 0)
            {
                return MonsterExpression.Defeated;
            }

            if (isHurt)
            {
                return MonsterExpression.Hurt;
            }

            if (maxHp > 0 && hp <= maxHp * lowHpFraction)
            {
                return MonsterExpression.LowHp;
            }

            return MonsterExpression.Neutral;
        }
    }
}
