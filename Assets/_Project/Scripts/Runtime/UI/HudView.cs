using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Events;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class HudView : MonoBehaviour
    {
        private const float LowHpFraction = 0.25f;

        [SerializeField] private float hpTweenDuration = 0.35f;
        [Tooltip("Optional: weakness hits flash the HP fill white.")]
        [SerializeField] private AttackResultEventChannelSO onAttackResolved;
        [Tooltip("Optional: reduce-motion skips the plate shake and low-HP pulse.")]
        [SerializeField] private UiMotionSettingsSO motionSettings;

        private UIDocument document;
        private Label nameLabel;
        private Label levelLabel;
        private Label hpLabel;
        private VisualElement hpFill;
        private VisualElement _hpTrail;
        private VisualElement _hpTrack;
        private VisualElement _nameplate;
        private int currentMaxHp = 1;
        private float _fillFraction = 1f;
        private float _trailFraction = 1f;
        private bool _pulsing;
        private bool _snapNextHp;

        private void OnEnable()
        {
            document = GetComponent<UIDocument>();
            var root = document.rootVisualElement;
            nameLabel = root.Q<Label>("monster-name-label");
            levelLabel = root.Q<Label>("monster-level-label");
            hpLabel = root.Q<Label>("hp-label");
            hpFill = root.Q<VisualElement>("hp-bar-fill");
            _hpTrail = root.Q<VisualElement>("hp-bar-trail");
            _hpTrack = root.Q<VisualElement>("hp-bar-track");
            _nameplate = root.Q<VisualElement>("nameplate");

            var sceneScrim = root.Q<VisualElement>("scene-scrim");
            if (sceneScrim != null)
            {
                UiMotion.FadeOut(sceneScrim, gameObject, 0.3f);
            }

            if (onAttackResolved != null)
            {
                onAttackResolved.RegisterListener(HandleAttackResolved);
            }
        }

        private void OnDisable()
        {
            if (onAttackResolved != null)
            {
                onAttackResolved.UnregisterListener(HandleAttackResolved);
            }

            DOTween.Kill(hpFill);
            DOTween.Kill(_hpTrail);
        }

        public void Bind(MonsterData monster, int level)
        {
            Bind(monster, level, null);
        }

        public void Bind(MonsterData monster, int level, ElementDefinition weakness)
        {
            nameLabel.text = monster.Name;
            ElementStyle.Apply(_nameplate, monster.Weakness);
            levelLabel.text = weakness != null && !string.IsNullOrEmpty(weakness.DisplayName)
                ? weakness.DisplayName
                : monster.Weakness.ToString();
            currentMaxHp = monster.MaxHp;
            SetHpImmediate(monster.MaxHp, monster.MaxHp);
            _snapNextHp = true;
        }

        public void SetHp(int hp, int maxHp)
        {
            if (_snapNextHp)
            {
                _snapNextHp = false;
                SetHpImmediate(hp, maxHp);
                _nameplate.EnableInClassList("nameplate--defeated", hp <= 0);
                return;
            }

            currentMaxHp = Mathf.Max(1, maxHp);
            hpLabel.text = $"{hp} / {maxHp}";
            float target = Mathf.Clamp01((float)hp / currentMaxHp);

            if (target < _fillFraction)
            {
                if (!UiMotionSettingsSO.Reduced(motionSettings))
                {
                    UiMotion.Shake(_nameplate, gameObject);
                }

                UiMotion.BarTo(hpFill, gameObject, _fillFraction, target, hpTweenDuration, 0f, Ease.OutCubic);
                UiMotion.BarTo(_hpTrail, gameObject, _trailFraction, target, 0.45f, 0.25f, Ease.InOutQuad);
            }
            else
            {
                UiMotion.BarTo(hpFill, gameObject, _fillFraction, target, hpTweenDuration, 0f, Ease.OutCubic);
                UiMotion.BarTo(_hpTrail, gameObject, _trailFraction, target, hpTweenDuration, 0f, Ease.OutCubic);
            }

            _fillFraction = target;
            _trailFraction = target;
            RefreshLowHp(target);
            _nameplate.EnableInClassList("nameplate--defeated", hp <= 0);
        }

        private void SetHpImmediate(int hp, int maxHp)
        {
            hpLabel.text = $"{hp} / {maxHp}";
            float fraction = Mathf.Clamp01((float)hp / Mathf.Max(1, maxHp));
            DOTween.Kill(hpFill);
            DOTween.Kill(_hpTrail);
            UiMotion.SetScaleX(hpFill, fraction);
            UiMotion.SetScaleX(_hpTrail, fraction);
            _fillFraction = fraction;
            _trailFraction = fraction;
            RefreshLowHp(fraction);
        }

        private void RefreshLowHp(float fraction)
        {
            bool low = fraction > 0f && fraction <= LowHpFraction;
            _hpTrack.EnableInClassList("bar--low", low);
            if (low && !_pulsing && !UiMotionSettingsSO.Reduced(motionSettings))
            {
                UiMotion.Pulse(_hpTrack, gameObject, 0.8f);
                _pulsing = true;
            }
            else if (!low && _pulsing)
            {
                DOTween.Kill(_hpTrack);
                _hpTrack.style.opacity = 1f;
                _pulsing = false;
            }
        }

        private void HandleAttackResolved(CommandResult result)
        {
            if (!result.Success || !result.WasWeakness)
            {
                return;
            }

            _hpTrack.AddToClassList("bar--flash");
            _hpTrack.schedule.Execute(() => _hpTrack.RemoveFromClassList("bar--flash")).StartingIn(80);
        }
    }
}
