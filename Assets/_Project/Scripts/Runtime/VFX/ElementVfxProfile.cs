namespace Spaa.VFX
{
    public readonly struct ElementVfxProfile
    {
        public const int ShapeOrb = 0;
        public const int ShapeRing = 1;
        public const int ShapeStar = 2;
        public const int ShapeHeart = 3;
        public const int ShapeSquare = 4;

        public int Shape { get; }
        public int BurstCount { get; }
        public float Lifetime { get; }
        public float Speed { get; }
        public float Size { get; }
        public float GravityModifier { get; }
        public float EmitRadius { get; }
        public bool Stretched { get; }
        public bool Shockwave { get; }

        public ElementVfxProfile(int shape, int burstCount, float lifetime, float speed, float size,
            float gravityModifier, float emitRadius, bool stretched = false, bool shockwave = false)
        {
            Shape = shape;
            BurstCount = burstCount;
            Lifetime = lifetime;
            Speed = speed;
            Size = size;
            GravityModifier = gravityModifier;
            EmitRadius = emitRadius;
            Stretched = stretched;
            Shockwave = shockwave;
        }
    }
}
