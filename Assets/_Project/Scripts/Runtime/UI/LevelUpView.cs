using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Events;
using Spaa.Flow;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class LevelUpView : MonoBehaviour
    {
        private const int ElementCount = 5;
        private const int BonusPercentPerLevel = 25;

        [SerializeField] private BattleController battleController;
        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [Tooltip("Confetti piece colors for the Level Up celebration (defaults = the 5 element colors).")]
        [SerializeField] private Color[] confettiColors =
        {
            new Color32(0xA7, 0x33, 0xD6, 0xFF),
            new Color32(0x33, 0x84, 0xD6, 0xFF),
            new Color32(0x23, 0x9C, 0x26, 0xFF),
            new Color32(0xE7, 0xD2, 0x25, 0xFF),
            new Color32(0xD6, 0x33, 0x33, 0xFF),
        };
        [SerializeField] private int confettiCount = 40;
        [Tooltip("Indexed by Element enum value; card display names.")]
        [SerializeField] private ElementDefinition[] elementDefinitions;
        [Tooltip("Optional: reduce-motion skips confetti and the title bob.")]
        [SerializeField] private UiMotionSettingsSO motionSettings;

        private VisualElement panel;
        private VisualElement _title;
        private VisualElement _subtitle;
        private VisualElement _grid;
        private VisualElement _featured;
        private VisualElement _banner;
        private readonly System.Random _confettiRandom = new System.Random();
        private readonly Button[] cards = new Button[ElementCount];
        private readonly Label[] labels = new Label[ElementCount];
        private readonly Label[] subtitles = new Label[ElementCount];
        private readonly Label[] _levels = new Label[ElementCount];
        private readonly System.Action[] clickHandlers = new System.Action[ElementCount];

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            panel = root.Q<VisualElement>("level-up-panel");
            _title = panel.Q<Label>(className: "level-up-title");
            _subtitle = panel.Q<Label>(className: "level-up-subtitle");
            _grid = root.Q<VisualElement>("level-up-grid");
            _featured = root.Q<VisualElement>("level-up-featured");
            _banner = root.Q<VisualElement>("victory-banner");

            for (int i = 0; i < ElementCount; i++)
            {
                cards[i] = root.Q<Button>($"level-up-card-{i}");
                labels[i] = root.Q<Label>($"level-up-card-{i}-text");
                subtitles[i] = root.Q<Label>($"level-up-card-{i}-subtitle");
                _levels[i] = root.Q<Label>($"level-up-card-{i}-levels");

                int index = i;
                clickHandlers[i] = () => HandleCardClicked((Element)index);
                cards[i].clicked += clickHandlers[i];
            }

            if (onRequestGameState != null)
            {
                onRequestGameState.RegisterListener(HandleStateRequested);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < ElementCount; i++)
            {
                cards[i].clicked -= clickHandlers[i];
            }

            if (onRequestGameState != null)
            {
                onRequestGameState.UnregisterListener(HandleStateRequested);
            }
        }

        private void HandleStateRequested(GameState state)
        {
            bool show = state == GameState.LevelUp;
            panel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            if (show)
            {
                BuildCards();
                PlayEntrance();
            }
            else
            {
                if (_title != null)
                {
                    DG.Tweening.DOTween.Kill(_title);
                }

                DG.Tweening.DOTween.Kill(_banner);
            }
        }

        private void PlayEntrance()
        {
            UiMotion.FadeIn(panel, gameObject);

            SetCardsVisible(false);
            _banner.style.display = DisplayStyle.Flex;
            UiMotion.VictoryBanner(_banner, gameObject, RevealCards);

            if (confettiColors != null && confettiColors.Length > 0 && !UiMotionSettingsSO.Reduced(motionSettings))
            {
                UiMotion.Confetti(panel, confettiColors, confettiCount, _confettiRandom, gameObject);
            }
        }

        private void RevealCards()
        {
            _banner.style.display = DisplayStyle.None;
            SetCardsVisible(true);
            if (_title != null)
            {
                if (UiMotionSettingsSO.Reduced(motionSettings))
                {
                    UiMotion.FadeIn(_title, gameObject, 0.15f);
                }
                else
                {
                    UiMotion.TitleBounce(_title, gameObject);
                }
            }

            UiMotion.FadeIn(_subtitle, gameObject);
            for (int i = 0; i < ElementCount; i++)
            {
                UiMotion.SlideUpIn(cards[i], gameObject, 160f, 0.1f + 0.07f * i);
            }
        }

        private void SetCardsVisible(bool visible)
        {
            var visibility = visible ? Visibility.Visible : Visibility.Hidden;
            _title.style.visibility = visibility;
            _subtitle.style.visibility = visibility;
            for (int i = 0; i < ElementCount; i++)
            {
                cards[i].style.visibility = visibility;
            }
        }

        private void BuildCards()
        {
            var levels = battleController.ElementLevels;
            var weakness = battleController.Monster != null ? battleController.Monster.Weakness : (Element?)null;

            for (int i = 0; i < ElementCount; i++)
            {
                cards[i].RemoveFromHierarchy();
            }

            for (int i = 0; i < ElementCount; i++)
            {
                var element = (Element)i;
                bool recommended = weakness.HasValue && weakness.Value == element;
                int level = levels.GetLevel(element);

                ElementStyle.Apply(cards[i], element);
                labels[i].text = ElementName(element);
                _levels[i].text = $"Lv {level} → {level + 1}";
                subtitles[i].text = recommended
                    ? $"Recommended · +{BonusPercentPerLevel}% damage"
                    : $"+{BonusPercentPerLevel}% damage";
                cards[i].EnableInClassList("level-up-card--recommended", recommended);
                (recommended ? _featured : _grid).Add(cards[i]);
            }

            _featured.style.display = weakness.HasValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private string ElementName(Element element)
        {
            int index = (int)element;
            if (elementDefinitions != null && index < elementDefinitions.Length && elementDefinitions[index] != null
                && !string.IsNullOrEmpty(elementDefinitions[index].DisplayName))
            {
                return elementDefinitions[index].DisplayName;
            }

            return element.ToString();
        }

        private void HandleCardClicked(Element element)
        {
            battleController.ApplyLevelUp(element);
            onRequestGameState?.Raise(GameState.Dashboard);
        }
    }
}
