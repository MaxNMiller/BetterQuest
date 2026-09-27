using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Events;
using Spaa.Flow;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ReminderView : MonoBehaviour
    {
        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [Tooltip("Optional: shows the end-of-day damage as a '−N HP' line (player HP itself stays hidden).")]
        [SerializeField] private IntEventChannelSO onPlayerDamaged;

        private VisualElement panel;
        private Button adjustHabitsButton;
        private Button continueButton;
        private Label _damageLabel;
        private int _lastDamage;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            panel = root.Q<VisualElement>("soft-reminder-panel");
            adjustHabitsButton = root.Q<Button>("adjust-habits-button");
            continueButton = root.Q<Button>("continue-button");
            _damageLabel = root.Q<Label>("reminder-damage");

            adjustHabitsButton.clicked += HandleReturnToMainMenu;
            continueButton.clicked += HandleReturnToMainMenu;

            if (onRequestGameState != null)
            {
                onRequestGameState.RegisterListener(HandleStateRequested);
            }

            if (onPlayerDamaged != null)
            {
                onPlayerDamaged.RegisterListener(HandlePlayerDamaged);
            }
        }

        private void OnDisable()
        {
            adjustHabitsButton.clicked -= HandleReturnToMainMenu;
            continueButton.clicked -= HandleReturnToMainMenu;

            if (onRequestGameState != null)
            {
                onRequestGameState.UnregisterListener(HandleStateRequested);
            }

            if (onPlayerDamaged != null)
            {
                onPlayerDamaged.UnregisterListener(HandlePlayerDamaged);
            }
        }

        private void HandlePlayerDamaged(int damage)
        {
            _lastDamage = damage;
        }

        private void HandleStateRequested(GameState state)
        {
            bool show = state == GameState.SoftReminder;
            panel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            if (show)
            {
                _damageLabel.text = $"The monster hit back: −{_lastDamage} HP";
                _damageLabel.style.display = _lastDamage > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                _lastDamage = 0;

                UiMotion.FadeIn(panel, gameObject);
                var card = panel.Q<VisualElement>(className: "reminder-card");
                if (card != null)
                {
                    UiMotion.PopIn(card, gameObject, 0.1f);
                }
            }
        }

        private void HandleReturnToMainMenu()
        {
            onRequestGameState?.Raise(GameState.MainMenu);
        }
    }
}
