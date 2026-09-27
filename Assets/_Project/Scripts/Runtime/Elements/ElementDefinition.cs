using UnityEngine;
using UnityEngine.UIElements;

namespace Spaa.Elements
{
    [CreateAssetMenu(menuName = "Spaa/Elements/Element Definition", fileName = "New Element Definition")]
    public class ElementDefinition : ScriptableObject
    {
        [SerializeField] private Element element;
        [SerializeField] private string displayName;
        [SerializeField] private Color color = Color.white;
        [Tooltip("Arena fog tint used by ArenaBuilder.")]
        [SerializeField] private Color arenaFogColor = Color.white;
        [Tooltip("Arena ground tint used by ArenaBuilder.")]
        [SerializeField] private Color arenaGroundColor = Color.white;
        [Tooltip("Arena key-light tint used by ArenaBuilder.")]
        [SerializeField] private Color arenaLightColor = Color.white;
        [Tooltip("UI icon (SVG VectorImage), used by C#-spawned UI such as the completion orb.")]
        [SerializeField] private VectorImage icon;
        [Tooltip("One-line description shown under the element tiles in the habit editor.")]
        [SerializeField] private string tagline;

        public Element Element => element;
        public VectorImage Icon => icon;
        public string Tagline => tagline;
        public string DisplayName => displayName;
        public Color Color => color;
        public Color ArenaFogColor => arenaFogColor;
        public Color ArenaGroundColor => arenaGroundColor;
        public Color ArenaLightColor => arenaLightColor;
    }
}
