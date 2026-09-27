using UnityEngine;
using Spaa.Elements;

namespace Spaa.Battle
{
    public static class ArenaLayout
    {
        public static ArenaPropSpec[] GetProps(Element element)
        {
            switch (element)
            {
                case Element.Rest:
                    return new[]
                    {
                        new ArenaPropSpec(PrimitiveType.Sphere, new Vector3(-4f, 3.5f, 3f), new Vector3(1.2f, 1.2f, 1.2f)),
                        new ArenaPropSpec(PrimitiveType.Sphere, new Vector3(4.5f, 4.2f, 2f), new Vector3(0.7f, 0.7f, 0.7f)),
                        new ArenaPropSpec(PrimitiveType.Cylinder, new Vector3(-3f, 1f, -3f), new Vector3(0.5f, 2f, 0.5f)),
                        new ArenaPropSpec(PrimitiveType.Cylinder, new Vector3(3f, 1f, -3f), new Vector3(0.5f, 2f, 0.5f)),
                    };

                case Element.SelfCare:
                    return new[]
                    {
                        new ArenaPropSpec(PrimitiveType.Cylinder, new Vector3(-3.5f, 0.05f, 2f), new Vector3(2f, 0.05f, 2f)),
                        new ArenaPropSpec(PrimitiveType.Cylinder, new Vector3(3.5f, 0.05f, -2f), new Vector3(1.5f, 0.05f, 1.5f)),
                        new ArenaPropSpec(PrimitiveType.Cylinder, new Vector3(0f, 0.05f, -4.5f), new Vector3(1.2f, 0.05f, 1.2f)),
                    };

                case Element.Food:
                    return new[]
                    {
                        new ArenaPropSpec(PrimitiveType.Cube, new Vector3(-4f, 0.5f, 3f), new Vector3(1f, 1f, 1f)),
                        new ArenaPropSpec(PrimitiveType.Sphere, new Vector3(-4f, 1.3f, 3f), new Vector3(0.5f, 0.5f, 0.5f)),
                        new ArenaPropSpec(PrimitiveType.Cube, new Vector3(4f, 0.5f, 3f), new Vector3(1f, 1f, 1f)),
                        new ArenaPropSpec(PrimitiveType.Sphere, new Vector3(4f, 1.3f, 3f), new Vector3(0.5f, 0.5f, 0.5f)),
                    };

                case Element.Rebuild:
                    return new[]
                    {
                        new ArenaPropSpec(PrimitiveType.Cube, new Vector3(-4f, 1.5f, -2f), new Vector3(1f, 3f, 1f)),
                        new ArenaPropSpec(PrimitiveType.Cube, new Vector3(4f, 1.5f, -2f), new Vector3(1f, 3f, 1f)),
                        new ArenaPropSpec(PrimitiveType.Cube, new Vector3(0f, 3.2f, -2f), new Vector3(9f, 0.6f, 1f)),
                    };

                case Element.Move:
                    return new[]
                    {
                        new ArenaPropSpec(PrimitiveType.Cube, new Vector3(-2.5f, 0.4f, 4f), new Vector3(1.5f, 0.8f, 0.3f)),
                        new ArenaPropSpec(PrimitiveType.Cube, new Vector3(0f, 0.4f, 4f), new Vector3(1.5f, 0.8f, 0.3f)),
                        new ArenaPropSpec(PrimitiveType.Cube, new Vector3(2.5f, 0.4f, 4f), new Vector3(1.5f, 0.8f, 0.3f)),
                    };

                default:
                    return System.Array.Empty<ArenaPropSpec>();
            }
        }
    }
}
