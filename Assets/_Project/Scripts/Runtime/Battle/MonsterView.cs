using DG.Tweening;
using UnityEngine;
using Spaa.Elements;
using Spaa.Events;

namespace Spaa.Battle
{
    public class MonsterView : MonoBehaviour
    {
        private static readonly int FlashId = Shader.PropertyToID("_Flash");
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        private static readonly int DissolveEdgeColorId = Shader.PropertyToID("_DissolveEdgeColor");

        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Renderer[] tintedRenderers;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 1.5f;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private float deathDuration = 0.6f;
        [Tooltip("Squash/stretch punch applied to the body on every hit (x2 on weakness hits).")]
        [SerializeField] private float hitPunch = 0.12f;
        [SerializeField] private AttackResultEventChannelSO onAttackResolved;
        [SerializeField] private VoidEventChannelSO onMonsterDefeated;
        [Tooltip("End-of-day monster attack: plays the Deal Damage clip.")]
        [SerializeField] private IntEventChannelSO onPlayerDamaged;
        [Tooltip("Optional: plays the authored Gloob clips. Null = procedural reactions only.")]
        [SerializeField] private MonsterAnimationPlayer animationPlayer;
        [Tooltip("Optional: swaps the Gloob's eye/mouth textures per element and expression.")]
        [SerializeField] private MonsterFaceView faceView;
        [Tooltip("How long the hurt face shows after a hit before returning to the element / low-HP face.")]
        [SerializeField] private float hurtFaceDuration = 0.7f;
        [Tooltip("Wait before the death dissolve starts, so the death clip reads first.")]
        [SerializeField] private float deathDissolveDelay = 0.9f;

        private Vector3 bodyBaseLocalPosition;
        private Vector3 _bodyBaseScale = Vector3.one;
        private Material[] _materials = System.Array.Empty<Material>();
        private float _flash;
        private float _dissolve;
        private Tween _flashTween;
        private Tween _punchTween;
        private Tween _hurtFaceTween;
        private Element _weakness;
        private int _hp = 1;
        private int _maxHp = 1;
        private bool _isHurt;
        private bool _isDefeated;

        private void Awake()
        {
            if (bodyRoot != null)
            {
                bodyBaseLocalPosition = bodyRoot.localPosition;
                _bodyBaseScale = bodyRoot.localScale;
            }

            var renderers = GetComponentsInChildren<Renderer>(true);
            _materials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                _materials[i] = renderers[i].material;
            }
        }

        private void OnEnable()
        {
            if (onAttackResolved != null)
            {
                onAttackResolved.RegisterListener(HandleAttackResolved);
            }

            if (onMonsterDefeated != null)
            {
                onMonsterDefeated.RegisterListener(HandleMonsterDefeated);
            }

            if (onPlayerDamaged != null)
            {
                onPlayerDamaged.RegisterListener(HandlePlayerDamaged);
            }
        }

        private void OnDisable()
        {
            if (onAttackResolved != null)
            {
                onAttackResolved.UnregisterListener(HandleAttackResolved);
            }

            if (onMonsterDefeated != null)
            {
                onMonsterDefeated.UnregisterListener(HandleMonsterDefeated);
            }

            if (onPlayerDamaged != null)
            {
                onPlayerDamaged.UnregisterListener(HandlePlayerDamaged);
            }

            DOTween.Kill(this);
            transform.DOKill();
            if (bodyRoot != null)
            {
                bodyRoot.DOKill();
            }
        }

        private void OnDestroy()
        {
            foreach (var material in _materials)
            {
                Destroy(material);
            }
        }

        private void Update()
        {
            if (bodyRoot == null)
            {
                return;
            }

            float offset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            bodyRoot.localPosition = bodyBaseLocalPosition + new Vector3(0f, offset, 0f);
        }

        public void Setup(ElementDefinition weakness)
        {
            if (tintedRenderers != null)
            {
                foreach (var tinted in tintedRenderers)
                {
                    if (tinted != null)
                    {
                        tinted.material.color = weakness.Color;
                    }
                }
            }

            foreach (var material in _materials)
            {
                material.SetColor(DissolveEdgeColorId, Color.Lerp(weakness.Color, Color.white, 0.5f));
            }

            SetFlash(0f);
            SetDissolve(0f);

            _weakness = weakness.Element;
            RefreshFace();
        }

        public void SetHealth(int hp, int maxHp)
        {
            _hp = hp;
            _maxHp = maxHp;
            RefreshFace();
        }

        private void RefreshFace()
        {
            if (faceView == null)
            {
                return;
            }

            float lowHpFraction = faceView.Library != null ? faceView.Library.LowHpFraction : MonsterExpressionRules.DefaultLowHpFraction;
            faceView.Show(MonsterExpressionRules.Resolve(_hp, _maxHp, _isHurt, lowHpFraction), _weakness);
        }

        private void HandlePlayerDamaged(int damage)
        {
            if (animationPlayer != null)
            {
                animationPlayer.Play(MonsterAnimation.DealDamage);
            }
        }

        private void HandleAttackResolved(CommandResult result)
        {
            if (!result.Success || _isDefeated)
            {
                return;
            }

            if (result.MonsterDefeated || _hp <= 0)
            {
                HandleMonsterDefeated();
                return;
            }

            if (animationPlayer != null)
            {
                animationPlayer.Play(MonsterAnimation.TakeDamage);
            }

            _isHurt = true;
            RefreshFace();
            _hurtFaceTween?.Kill();
            _hurtFaceTween = DOVirtual.DelayedCall(hurtFaceDuration, () =>
                {
                    _isHurt = false;
                    RefreshFace();
                })
                .SetTarget(this)
                .SetLink(gameObject);

            _flashTween?.Kill();
            SetFlash(1f);
            _flashTween = DOTween.To(() => _flash, SetFlash, 0f, flashDuration)
                .SetEase(Ease.OutQuad)
                .SetTarget(this)
                .SetLink(gameObject);

            if (bodyRoot != null)
            {
                _punchTween?.Kill(true);
                float strength = result.WasWeakness ? hitPunch * 2f : hitPunch;
                bodyRoot.localScale = _bodyBaseScale;
                _punchTween = bodyRoot.DOPunchScale(new Vector3(strength, -strength, strength), 0.3f, 8, 0.6f)
                    .SetLink(gameObject);
            }
        }

        private void HandleMonsterDefeated()
        {
            if (_isDefeated)
            {
                return;
            }

            _isDefeated = true;
            DOTween.Kill(this);
            _punchTween?.Kill();
            if (bodyRoot != null)
            {
                bodyRoot.localScale = _bodyBaseScale;
            }
            SetFlash(0f);

            _hp = 0;
            _isHurt = false;
            RefreshFace();
            float clipLength = animationPlayer != null ? animationPlayer.Play(MonsterAnimation.Death) : 0f;

            var sequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject);
            if (clipLength <= 0f)
            {
                sequence.Join(transform.DOShakePosition(deathDuration * 0.5f, 0.2f, 30));
            }
            sequence.AppendInterval(Mathf.Max(deathDissolveDelay, clipLength));
            sequence.Append(DOTween.To(() => _dissolve, SetDissolve, 1f, deathDuration * 2f).SetEase(Ease.InQuad));
            sequence.OnComplete(() => gameObject.SetActive(false));
        }

        private void SetFlash(float value)
        {
            _flash = value;
            foreach (var material in _materials)
            {
                material.SetFloat(FlashId, value);
            }
        }

        private void SetDissolve(float value)
        {
            _dissolve = value;
            foreach (var material in _materials)
            {
                material.SetFloat(DissolveId, value);
            }
        }
    }
}
