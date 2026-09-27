using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Events;
using Spaa.Flow;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SplashView : MonoBehaviour
    {
        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float popDuration = 0.3f;

        private VisualElement panel;
        private VisualElement logo;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            panel = root.Q<VisualElement>("splash-panel");
            logo = root.Q<VisualElement>("splash-logo");
            StartCoroutine(PlaySplash());
        }

        private IEnumerator PlaySplash()
        {
            UiMotion.PopIn(logo, gameObject);
            yield return new WaitForSeconds(popDuration + holdDuration);

            panel.style.display = DisplayStyle.None;
            onRequestGameState?.Raise(GameState.MainMenu);
        }
    }
}
