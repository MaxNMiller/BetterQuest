using Spaa.Elements;

namespace Spaa.VFX
{
    public static class ElementVfxProfiles
    {
        public const int MaxParticlesPerEffect = 48;
        public const float WeaknessMultiplier = 1.5f;

        private static readonly ElementVfxProfile Rest =
            new ElementVfxProfile(ElementVfxProfile.ShapeStar, 18, 1.4f, 1.2f, 0.45f, -0.05f, 0.8f);
        private static readonly ElementVfxProfile SelfCare =
            new ElementVfxProfile(ElementVfxProfile.ShapeRing, 22, 1.2f, 1.8f, 0.5f, -0.25f, 0.6f);
        private static readonly ElementVfxProfile Food =
            new ElementVfxProfile(ElementVfxProfile.ShapeOrb, 20, 0.8f, 4.5f, 0.5f, 0.7f, 0.3f);
        private static readonly ElementVfxProfile Rebuild =
            new ElementVfxProfile(ElementVfxProfile.ShapeHeart, 16, 1.3f, 1.5f, 0.55f, -0.3f, 0.7f);
        private static readonly ElementVfxProfile Move =
            new ElementVfxProfile(ElementVfxProfile.ShapeOrb, 28, 0.45f, 9f, 0.22f, 0f, 0.2f, stretched: true, shockwave: true);

        public static readonly ElementVfxProfile HitBurst =
            new ElementVfxProfile(ElementVfxProfile.ShapeStar, 12, 0.35f, 5f, 0.5f, 0f, 0.1f);

        public static ElementVfxProfile For(Element element)
        {
            switch (element)
            {
                case Element.Rest: return Rest;
                case Element.SelfCare: return SelfCare;
                case Element.Food: return Food;
                case Element.Rebuild: return Rebuild;
                case Element.Move: return Move;
                default: return HitBurst;
            }
        }

        public static int EmitCount(ElementVfxProfile profile, bool isWeakness)
        {
            float count = isWeakness ? profile.BurstCount * WeaknessMultiplier : profile.BurstCount;
            int rounded = (int)System.Math.Round(count, System.MidpointRounding.AwayFromZero);
            return System.Math.Min(rounded, MaxParticlesPerEffect);
        }
    }
}
