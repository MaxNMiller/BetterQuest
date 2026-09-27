using UnityEngine;

namespace Spaa.UI
{
    [CreateAssetMenu(menuName = "Spaa/UI/Motion Settings", fileName = "UiMotionSettings")]
    public class UiMotionSettingsSO : ScriptableObject
    {
        [Tooltip("Skips orb, shake, bob, pulse and confetti.")]
        [SerializeField] private bool reduceMotion;
        [Tooltip("Multiplies decorative animation durations (1 = spec timing).")]
        [SerializeField] private float durationScale = 1f;

        public bool ReduceMotion => reduceMotion;
        public float DurationScale => Mathf.Max(0.01f, durationScale);

        public static bool Reduced(UiMotionSettingsSO settings)
        {
            return settings != null && settings.ReduceMotion;
        }

        public static float Scale(UiMotionSettingsSO settings)
        {
            return settings != null ? settings.DurationScale : 1f;
        }
    }
}
