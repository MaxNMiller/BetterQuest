namespace Spaa.Battle
{
    public static class CameraImpulseMath
    {
        public const float NeutralForce = 0.25f;
        public const float WeaknessForce = 0.5f;
        public const float DefeatForce = 0.9f;

        public static float ForceFor(CommandResult result)
        {
            if (!result.Success)
            {
                return 0f;
            }

            if (result.MonsterDefeated)
            {
                return DefeatForce;
            }

            return result.WasWeakness ? WeaknessForce : NeutralForce;
        }
    }
}
