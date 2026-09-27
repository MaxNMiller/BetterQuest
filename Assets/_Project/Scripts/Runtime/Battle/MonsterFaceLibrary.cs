using UnityEngine;
using Spaa.Elements;

namespace Spaa.Battle
{
    [CreateAssetMenu(menuName = "Spaa/Monster Face Library", fileName = "MonsterFaceLibrary")]
    public class MonsterFaceLibrary : ScriptableObject
    {
        [Tooltip("Neutral face per element, indexed by the Element enum (Rest, SelfCare, Food, Rebuild, Move).")]
        [SerializeField] private MonsterFace[] elementFaces = new MonsterFace[5];
        [Tooltip("Shown briefly after every hit. Empty parts keep the element face.")]
        [SerializeField] private MonsterFace hurtFace;
        [Tooltip("Shown while HP is at or below the low-HP fraction. Empty parts keep the element face.")]
        [SerializeField] private MonsterFace lowHpFace;
        [SerializeField] private MonsterFace defeatedFace;
        [Tooltip("Multiplied into the (white) face textures.")]
        [SerializeField] private Color faceColor = new Color(0.11f, 0.12f, 0.23f, 1f);
        [Tooltip("HP fraction at or below which the LowHp face shows.")]
        [Range(0f, 1f)]
        [SerializeField] private float lowHpFraction = MonsterExpressionRules.DefaultLowHpFraction;

        public Color FaceColor => faceColor;
        public float LowHpFraction => lowHpFraction;

        public MonsterFace GetElementFace(Element element)
        {
            int index = (int)element;
            return elementFaces != null && index >= 0 && index < elementFaces.Length ? elementFaces[index] : default;
        }

        public MonsterFace GetFace(MonsterExpression expression, Element element)
        {
            var baseFace = GetElementFace(element);
            switch (expression)
            {
                case MonsterExpression.Hurt:
                    return MonsterFace.Layer(baseFace, hurtFace);
                case MonsterExpression.LowHp:
                    return MonsterFace.Layer(baseFace, lowHpFace);
                case MonsterExpression.Defeated:
                    return MonsterFace.Layer(baseFace, defeatedFace);
                default:
                    return baseFace;
            }
        }
    }
}
