using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Battle;
using Spaa.Events;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ToastView : MonoBehaviour
    {
        [SerializeField] private ToastMessagePoolSO messagePool;
        [SerializeField] private AttackResultEventChannelSO onAttackResolved;
        [SerializeField] private float riseDistance = 40f;
        [SerializeField] private float toastDuration = 1.2f;
        [SerializeField] private float fadeDuration = 0.25f;

        private VisualElement toastContainer;
        private VisualElement _calloutLayer;
        private VisualElement _monsterAnchor;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            toastContainer = root.Q("toast-container");
            _calloutLayer = root.Q("safe-area-root") ?? root;
            _monsterAnchor = root.Q("monster-anchor");
        }

        private void OnEnable()
        {
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
        }

        private void HandleAttackResolved(CommandResult result)
        {
            if (!result.Success || toastContainer == null || messagePool == null)
            {
                return;
            }

            var pool = result.WasWeakness ? messagePool.WeaknessMessages : messagePool.NeutralMessages;
            if (pool == null || pool.Length == 0)
            {
                return;
            }

            string message = pool[Random.Range(0, pool.Length)];
            StartCoroutine(ShowToast(message));
            ShowDamageNumber(result.DamageDealt, result.WasWeakness);

            if (result.WasWeakness)
            {
                ShowCallout();
            }
        }

        private void ShowDamageNumber(int damage, bool weakness)
        {
            if (_calloutLayer == null || _monsterAnchor == null || damage <= 0)
            {
                return;
            }

            var number = new Label(weakness ? $"−{damage}!" : $"−{damage}") { pickingMode = PickingMode.Ignore };
            number.AddToClassList("dmg-number");
            number.EnableInClassList("dmg-number--weak", weakness);
            Vector2 anchor = _calloutLayer.WorldToLocal(_monsterAnchor.worldBound.center);
            number.style.left = anchor.x;
            number.style.top = anchor.y - 200f;
            _calloutLayer.Add(number);
            UiMotion.FloatUp(number, gameObject);
        }

        private void ShowCallout()
        {
            if (_calloutLayer == null)
            {
                return;
            }

            var callout = new Label("Super effective!") { pickingMode = PickingMode.Ignore };
            callout.AddToClassList("callout");
            _calloutLayer.Add(callout);
            UiMotion.Callout(callout, gameObject);
        }

        private IEnumerator ShowToast(string message)
        {
            var label = new Label(message);
            label.AddToClassList("toast");
            toastContainer.Add(label);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                label.style.opacity = t;
                label.style.translate = new Translate(0, -riseDistance * t);
                yield return null;
            }

            yield return new WaitForSeconds(toastDuration);

            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                label.style.opacity = 1f - t;
                yield return null;
            }

            toastContainer.Remove(label);
        }
    }
}
