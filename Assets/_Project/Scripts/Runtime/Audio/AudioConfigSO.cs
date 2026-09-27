using UnityEngine;

namespace Spaa.Audio
{
    [CreateAssetMenu(menuName = "Spaa/Config/Audio Config", fileName = "AudioConfig")]
    public class AudioConfigSO : ScriptableObject
    {
        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip battleMusic;
        [Tooltip("Music volume (0-1).")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
        [Tooltip("Seconds to fade the scene's music in on load.")]
        [SerializeField, Min(0f)] private float musicFadeIn = 1f;

        [Header("SFX")]
        [Tooltip("One is picked at random per successful attack (never the same twice in a row).")]
        [SerializeField] private AudioClip[] attackClips = new AudioClip[0];
        [SerializeField] private AudioClip levelUp;
        [SerializeField] private AudioClip popup;
        [SerializeField] private AudioClip uiSelect;
        [Tooltip("SFX volume (0-1).")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.9f;

        public float MusicVolume => musicVolume;
        public float MusicFadeIn => musicFadeIn;
        public float SfxVolume => sfxVolume;
        public int AttackClipCount => attackClips != null ? attackClips.Length : 0;

        public AudioClip Music(MusicTrack track)
        {
            switch (track)
            {
                case MusicTrack.Menu:
                    return menuMusic;
                case MusicTrack.Battle:
                    return battleMusic;
                default:
                    return null;
            }
        }

        public AudioClip AttackClip(int index)
        {
            return index >= 0 && index < AttackClipCount ? attackClips[index] : null;
        }

        public AudioClip Clip(SfxCue cue)
        {
            switch (cue)
            {
                case SfxCue.LevelUp:
                    return levelUp;
                case SfxCue.Popup:
                    return popup;
                case SfxCue.UiSelect:
                    return uiSelect;
                default:
                    return null;
            }
        }
    }
}
