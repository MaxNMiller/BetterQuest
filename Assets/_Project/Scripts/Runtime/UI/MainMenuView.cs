using UnityEngine;
using UnityEngine.UIElements;
using Spaa.DayCycle;
using Spaa.Elements;
using Spaa.Events;
using Spaa.Flow;
using Spaa.Habits;
using Spaa.Save;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [Tooltip("Authored default habits, seeded into the save on first launch (see HabitRoster).")]
        [SerializeField] private HabitDefinition[] habitPool;
        [Tooltip("Indexed by Element enum value; display names for the plaque.")]
        [SerializeField] private ElementDefinition[] elementDefinitions;
        [Tooltip("Optional title artwork. When set it replaces the text lockup on the menu and splash.")]
        [SerializeField] private Texture2D titleLogo;

        private VisualElement panel;
        private Button playButton;
        private Button habitMenuButton;
        private Button quitButton;
        private Button _dashboardButton;
        private VisualElement _buttonRow;
        private Label playBlockedLabel;

        private VisualElement _titleBlock;
        private VisualElement _plaque;
        private VisualElement _gem;
        private VisualElement _gemIcon;
        private Label _heading;
        private Label _name;
        private Label _detail;
        private VisualElement _chip;
        private Label _chipLabel;
        private VisualElement _missing;
        private VisualElement _playIcon;
        private Label _playLabel;

        private ISaveService _saveService;
        private IDayClock _dayClock;

        private void OnEnable()
        {
            _saveService = new JsonSaveService(SaveLocation.DefaultFilePath);
            _dayClock = new SystemDayClock();

            var root = GetComponent<UIDocument>().rootVisualElement;
            panel = root.Q<VisualElement>("main-menu-panel");
            playButton = root.Q<Button>("play-button");
            habitMenuButton = root.Q<Button>("habit-menu-button");
            quitButton = root.Q<Button>("quit-button");
            _dashboardButton = root.Q<Button>("dashboard-button");
            _buttonRow = habitMenuButton.parent;
            playBlockedLabel = root.Q<Label>("play-blocked-label");

            _titleBlock = root.Q<VisualElement>("menu-title-block");
            _plaque = root.Q<VisualElement>("today-plaque");
            _gem = root.Q<VisualElement>("today-gem");
            _gemIcon = root.Q<VisualElement>("today-gem-icon");
            _heading = root.Q<Label>("today-heading");
            _name = root.Q<Label>("today-name");
            _detail = root.Q<Label>("today-detail");
            _chip = root.Q<VisualElement>("today-chip");
            _chipLabel = root.Q<Label>("today-chip-label");
            _missing = root.Q<VisualElement>("today-missing");
            _playIcon = root.Q<VisualElement>("play-icon");
            _playLabel = root.Q<Label>("play-label");

            ApplyTitleLogo(root);

            playButton.clicked += HandlePlayClicked;
            habitMenuButton.clicked += HandleHabitMenuClicked;
            _dashboardButton.clicked += HandleDashboardClicked;
            quitButton.clicked += HandleQuitClicked;

            quitButton.style.display = Application.platform == RuntimePlatform.IPhonePlayer
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            if (onRequestGameState != null)
            {
                onRequestGameState.RegisterListener(HandleStateRequested);
            }
        }

        private void OnDisable()
        {
            playButton.clicked -= HandlePlayClicked;
            habitMenuButton.clicked -= HandleHabitMenuClicked;
            _dashboardButton.clicked -= HandleDashboardClicked;
            quitButton.clicked -= HandleQuitClicked;

            if (onRequestGameState != null)
            {
                onRequestGameState.UnregisterListener(HandleStateRequested);
            }
        }

        private void ApplyTitleLogo(VisualElement root)
        {
            if (titleLogo == null)
            {
                return;
            }

            var title = root.Q<VisualElement>("menu-title");
            title.AddToClassList("menu-title--image");
            title.style.backgroundImage = new StyleBackground(titleLogo);

            var splashLogo = root.Q<Label>("splash-logo");
            if (splashLogo != null)
            {
                splashLogo.text = string.Empty;
                splashLogo.AddToClassList("splash-logo--image");
                splashLogo.style.backgroundImage = new StyleBackground(titleLogo);
            }
        }

        private void HandleStateRequested(GameState state)
        {
            bool show = state == GameState.MainMenu;
            panel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            if (show)
            {
                RefreshPlayGate();

                UiMotion.PopIn(_titleBlock, gameObject);
                UiMotion.SlideUpIn(_plaque, gameObject, 120f, 0.1f);
                UiMotion.SlideUpIn(playButton, gameObject, 120f, 0.18f);
                UiMotion.SlideUpIn(_buttonRow, gameObject, 120f, 0.26f);
                UiMotion.SlideUpIn(quitButton, gameObject, 120f, 0.34f);
            }
        }

        private PlayBlockReason RefreshPlayGate()
        {
            var saveData = _saveService.Load();
            if (HabitRoster.EnsureSeeded(saveData, habitPool))
            {
                _saveService.Save(saveData);
            }

            string today = _dayClock.GetCurrentDayKey();
            var reason = PlayGate.Evaluate(saveData, today);
            playButton.SetEnabled(reason == PlayBlockReason.None);
            ShowStatus(TodayStatusResolver.Resolve(saveData, today));
            return reason;
        }

        private void ShowStatus(TodayStatus status)
        {
            _gem.RemoveFromClassList("today-gem--neutral");
            _gem.RemoveFromClassList("today-gem--success");
            _gem.RemoveFromClassList("today-gem--warning");
            _gemIcon.RemoveFromClassList("icon--rune");
            _gemIcon.RemoveFromClassList("icon--check");
            _gemIcon.RemoveFromClassList("icon--warning");
            _plaque.RemoveFromClassList("today-plaque--success");
            ElementStyle.Clear(_plaque);
            _missing.Clear();
            _chip.style.display = DisplayStyle.None;
            _missing.style.display = DisplayStyle.None;
            playBlockedLabel.style.display = DisplayStyle.None;
            _playIcon.RemoveFromClassList("icon--moon");
            _playIcon.RemoveFromClassList("icon--star");
            _playIcon.AddToClassList("icon--sword");
            _heading.text = "TODAY'S FOE";
            _playLabel.text = "Battle!";

            switch (status.Kind)
            {
                case TodayStatusKind.InProgress:
                case TodayStatusKind.LevelUpPending:
                    ElementStyle.Apply(_plaque, status.Weakness.GetValueOrDefault());
                    _name.text = status.MonsterName;
                    _chip.style.display = DisplayStyle.Flex;
                    _chipLabel.text = "WEAK · " + ElementName(status.Weakness.GetValueOrDefault());
                    if (status.Kind == TodayStatusKind.LevelUpPending)
                    {
                        _detail.text = "Defeated! Your reward is waiting.";
                        _playIcon.RemoveFromClassList("icon--sword");
                        _playIcon.AddToClassList("icon--star");
                        _playLabel.text = "Claim Reward";
                    }
                    else
                    {
                        _detail.text = "Battle in progress";
                        _playLabel.text = "Continue";
                    }

                    break;

                case TodayStatusKind.Defeated:
                    _gem.AddToClassList("today-gem--success");
                    _gemIcon.AddToClassList("icon--check");
                    _plaque.AddToClassList("today-plaque--success");
                    _name.text = "Victory! Today's foe is defeated.";
                    _detail.text = "A new monster arrives tomorrow.";
                    _playIcon.RemoveFromClassList("icon--sword");
                    _playIcon.AddToClassList("icon--moon");
                    _playLabel.text = "Resting until tomorrow";
                    break;

                case TodayStatusKind.MissingHabits:
                    _gem.AddToClassList("today-gem--warning");
                    _gemIcon.AddToClassList("icon--warning");
                    _heading.text = "CAN'T BATTLE YET";
                    _name.text = "Every element needs a habit.";
                    _detail.text = "Open the Habit Journal and add one for:";
                    _missing.style.display = DisplayStyle.Flex;
                    foreach (var element in status.MissingElements)
                    {
                        _missing.Add(CreateMissingItem(element));
                    }

                    break;

                default:
                    _gem.AddToClassList("today-gem--neutral");
                    _gemIcon.AddToClassList("icon--rune");
                    _name.text = "A monster stirs in the dungeon…";
                    _detail.text = "Finish real-life habits to attack it.";
                    break;
            }
        }

        private VisualElement CreateMissingItem(Element element)
        {
            var item = new VisualElement();
            item.AddToClassList("today-missing__item");
            ElementStyle.Apply(item, element);
            var icon = new VisualElement();
            icon.AddToClassList("el-icon");
            var label = new Label(ElementName(element));
            label.AddToClassList("t-caption");
            item.Add(icon);
            item.Add(label);
            return item;
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

        private void HandlePlayClicked()
        {
            if (RefreshPlayGate() != PlayBlockReason.None)
            {
                return;
            }

            if (onRequestGameState != null)
            {
                onRequestGameState.Raise(GameState.Battle);
            }
        }

        private void HandleHabitMenuClicked()
        {
            if (onRequestGameState != null)
            {
                onRequestGameState.Raise(GameState.HabitMenu);
            }
        }

        private void HandleDashboardClicked()
        {
            if (onRequestGameState != null)
            {
                onRequestGameState.Raise(GameState.Dashboard);
            }
        }

        private void HandleQuitClicked()
        {
            Application.Quit();
        }
    }
}
