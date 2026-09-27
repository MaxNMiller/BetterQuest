using UnityEngine;
using Spaa.Battle;
using Spaa.UI;

namespace Spaa.Tutorial
{
    [CreateAssetMenu(menuName = "Spaa/Tutorial/Config", fileName = "TutorialConfig")]
    public class TutorialConfigSO : ScriptableObject
    {
        [Tooltip("Kill switch: when off, no overlay shows and tutorial.json is never written.")]
        [SerializeField] private bool enabled = true;
        [Tooltip("Bump when the script changes meaningfully. Players who finished an older version replay only if Replay On Version Bump is on.")]
        [SerializeField] private int version = 1;
        [SerializeField] private bool replayOnVersionBump;
        [SerializeField] private TutorialScriptSO script;
        [SerializeField] private string guideName = "Wombino the Wise";
        [Tooltip("Opened by the Learn more button.")]
        [SerializeField] private string learnMoreUrl = "https://www.metabolicmind.org/thinksmart/";
        [Tooltip("Wombino wears the Gloob's own eye and mouth textures (read-only use).")]
        [SerializeField] private MonsterFaceLibrary faceLibrary;
        [Tooltip("Optional dedicated Wombino portrait (square). When set it replaces the tinted body and Gloob face.")]
        [SerializeField] private Texture2D portrait;
        [Tooltip("Reduce-motion switch shared with the rest of the UI.")]
        [SerializeField] private UiMotionSettingsSO motionSettings;
        [Tooltip("Space between the target and the edge of the spotlight hole, in panel pixels.")]
        [SerializeField] private float spotlightPadding = 16f;
        [Tooltip("Space between the dialogue and the hole or the safe-area edge, in panel pixels.")]
        [SerializeField] private float dialogueGap = 24f;
        [Tooltip("0 = show lines instantly.")]
        [SerializeField] private float typewriterCharsPerSecond = 45f;

        public bool Enabled => enabled;
        public int Version => version;
        public bool ReplayOnVersionBump => replayOnVersionBump;
        public TutorialScriptSO Script => script;
        public string GuideName => guideName;
        public string LearnMoreUrl => learnMoreUrl;
        public MonsterFaceLibrary FaceLibrary => faceLibrary;
        public Texture2D Portrait => portrait;
        public UiMotionSettingsSO MotionSettings => motionSettings;
        public float SpotlightPadding => spotlightPadding;
        public float DialogueGap => dialogueGap;
        public float TypewriterCharsPerSecond => Mathf.Max(0f, typewriterCharsPerSecond);
    }
}
