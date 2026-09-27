using UnityEngine;

namespace Spaa.Battle
{
    public readonly struct ArenaPropSpec
    {
        public PrimitiveType Primitive { get; }
        public Vector3 LocalPosition { get; }
        public Vector3 LocalScale { get; }
        public Vector3 LocalEulerAngles { get; }

        public ArenaPropSpec(PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Vector3 localEulerAngles = default)
        {
            Primitive = primitive;
            LocalPosition = localPosition;
            LocalScale = localScale;
            LocalEulerAngles = localEulerAngles;
        }
    }
}
