using DG.Tweening;
using UnityEngine;
using Spaa.Battle;
using Spaa.Events;
using Spaa.Flow;

namespace Spaa.Audio
{
    public class SceneAudio : MonoBehaviour
    {
        [SerializeField] private AudioConfigSO config;
        [Tooltip("Music looped while this scene is loaded.")]
        [SerializeField] private MusicTrack music = MusicTrack.None;
        [Tooltip("Seed for attack-variation picks; 0 = time-based.")]
        [SerializeField] private int randomSeed;

        [Header("Channels (optional)")]
        [SerializeField] private AttackResultEventChannelSO onAttackResolved;
        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [SerializeField] private VoidEventChannelSO onHabitEditorOpened;
        [Tooltip("Raised by views that pop up a modal/dialog without a game-state change.")]
        [SerializeField] private VoidEventChannelSO onPopupShown;
        [Tooltip("Raised by UiButtonSprings on every button click.")]
        [SerializeField] private VoidEventChannelSO onUiButtonClicked;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private ClipPicker _attackPicker;

        private void Awake()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f;

            int seed = randomSeed != 0 ? randomSeed : System.Environment.TickCount;
            _attackPicker = new ClipPicker(new System.Random(seed));
        }

        private void Start()
        {
            if (config == null)
            {
                return;
            }

            var clip = config.Music(music);
            if (clip == null)
            {
                return;
            }

            _musicSource.clip = clip;
            _musicSource.volume = 0f;
            _musicSource.Play();
            DOTween.To(() => _musicSource.volume, v => _musicSource.volume = v, config.MusicVolume, config.MusicFadeIn)
                .SetTarget(_musicSource)
                .SetLink(gameObject);
        }

        private void OnEnable()
        {
            if (onAttackResolved != null)
            {
                onAttackResolved.RegisterListener(HandleAttackResolved);
            }

            if (onRequestGameState != null)
            {
                onRequestGameState.RegisterListener(HandleStateRequested);
            }

            if (onHabitEditorOpened != null)
            {
                onHabitEditorOpened.RegisterListener(HandlePopupShown);
            }

            if (onPopupShown != null)
            {
                onPopupShown.RegisterListener(HandlePopupShown);
            }

            if (onUiButtonClicked != null)
            {
                onUiButtonClicked.RegisterListener(HandleUiButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (onAttackResolved != null)
            {
                onAttackResolved.UnregisterListener(HandleAttackResolved);
            }

            if (onRequestGameState != null)
            {
                onRequestGameState.UnregisterListener(HandleStateRequested);
            }

            if (onHabitEditorOpened != null)
            {
                onHabitEditorOpened.UnregisterListener(HandlePopupShown);
            }

            if (onPopupShown != null)
            {
                onPopupShown.UnregisterListener(HandlePopupShown);
            }

            if (onUiButtonClicked != null)
            {
                onUiButtonClicked.UnregisterListener(HandleUiButtonClicked);
            }

            if (_musicSource != null)
            {
                DOTween.Kill(_musicSource);
            }
        }

        private void HandleAttackResolved(CommandResult result)
        {
            if (AudioCueRules.ForAttack(result) != SfxCue.Attack || config == null)
            {
                return;
            }

            int index = _attackPicker.Next(config.AttackClipCount);
            PlayOneShot(config.AttackClip(index));
        }

        private void HandleStateRequested(GameState state)
        {
            Play(AudioCueRules.ForState(state));
        }

        private void HandlePopupShown()
        {
            Play(SfxCue.Popup);
        }

        private void HandleUiButtonClicked()
        {
            Play(SfxCue.UiSelect);
        }

        private void Play(SfxCue cue)
        {
            if (cue == SfxCue.None || config == null)
            {
                return;
            }

            PlayOneShot(config.Clip(cue));
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, config.SfxVolume);
            }
        }
    }
}
