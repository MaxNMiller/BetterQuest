using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Spaa.Battle;
using Spaa.DayCycle;
using Spaa.DebugTools;
using Spaa.Elements;
using Spaa.Events;
using Spaa.Flow;
using Spaa.Save;
using Spaa.Tutorial;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DebugPanelView : MonoBehaviour
    {
        private const int ElementCount = 5;
        private const int TripleTapCount = 3;
        private const float TripleTapWindow = 0.6f;

        [SerializeField] private BattleController battleController;
        [Tooltip("Menu mode only: raised with MainMenu after a save change so the menu refreshes its Play gate.")]
        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [Tooltip("Optional. Enables Reset Tutorial (writes a first-run tutorial.json).")]
        [SerializeField] private TutorialConfigSO tutorialConfig;

        private VisualElement panel;
        private VisualElement tapZone;
        private Toggle takeDamageToggle;
        private Button forceEndOfDayButton;
        private Button advanceDayButton;
        private Button killMonsterButton;
        private Button resetSaveButton;
        private Button seedHistoryButton;
        private Button clearHistoryButton;
        private Button resetTutorialButton;
        private readonly Button[] levelUpButtons = new Button[ElementCount];
        private readonly System.Action[] levelUpHandlers = new System.Action[ElementCount];
        private bool visible;
        private int tapCount;
        private float firstTapTime;

        private bool HasBattle => battleController != null;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            panel = root.Q<VisualElement>("debug-panel");
            tapZone = root.Q<VisualElement>("debug-tap-zone");
            takeDamageToggle = root.Q<Toggle>("debug-take-damage-toggle");
            forceEndOfDayButton = root.Q<Button>("debug-force-end-of-day-button");
            advanceDayButton = root.Q<Button>("debug-advance-day-button");
            killMonsterButton = root.Q<Button>("debug-kill-monster-button");
            resetSaveButton = root.Q<Button>("debug-reset-save-button");
            seedHistoryButton = root.Q<Button>("debug-seed-history-button");
            clearHistoryButton = root.Q<Button>("debug-clear-history-button");
            resetTutorialButton = root.Q<Button>("debug-reset-tutorial-button");

            tapZone.RegisterCallback<PointerDownEvent>(HandleTapZonePointerDown);
            advanceDayButton.clicked += HandleAdvanceDay;
            resetSaveButton.clicked += HandleResetSave;
            seedHistoryButton.clicked += HandleSeedHistory;
            clearHistoryButton.clicked += HandleClearHistory;
            if (resetTutorialButton != null)
            {
                resetTutorialButton.clicked += HandleResetTutorial;
            }

            if (HasBattle)
            {
                forceEndOfDayButton.clicked += battleController.ForceEndOfDay;
                killMonsterButton.clicked += battleController.KillMonster;
                takeDamageToggle.RegisterValueChangedCallback(HandleTakeDamageToggled);

                for (int i = 0; i < ElementCount; i++)
                {
                    var element = (Element)i;
                    levelUpButtons[i] = root.Q<Button>($"debug-levelup-{i}-button");
                    levelUpHandlers[i] = () => battleController.DebugLevelUp(element);
                    levelUpButtons[i].clicked += levelUpHandlers[i];
                }
            }
            else
            {
                forceEndOfDayButton.style.display = DisplayStyle.None;
                killMonsterButton.style.display = DisplayStyle.None;
                root.Q<VisualElement>("debug-take-damage-row").style.display = DisplayStyle.None;
                root.Q<VisualElement>("debug-levelup-section").style.display = DisplayStyle.None;
            }

            SetVisible(false);
        }

        private void OnDisable()
        {
            tapZone.UnregisterCallback<PointerDownEvent>(HandleTapZonePointerDown);
            advanceDayButton.clicked -= HandleAdvanceDay;
            resetSaveButton.clicked -= HandleResetSave;
            seedHistoryButton.clicked -= HandleSeedHistory;
            clearHistoryButton.clicked -= HandleClearHistory;
            if (resetTutorialButton != null)
            {
                resetTutorialButton.clicked -= HandleResetTutorial;
            }

            if (HasBattle)
            {
                forceEndOfDayButton.clicked -= battleController.ForceEndOfDay;
                killMonsterButton.clicked -= battleController.KillMonster;
                takeDamageToggle.UnregisterValueChangedCallback(HandleTakeDamageToggled);

                for (int i = 0; i < ElementCount; i++)
                {
                    if (levelUpButtons[i] != null)
                    {
                        levelUpButtons[i].clicked -= levelUpHandlers[i];
                    }
                }
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                SetVisible(!visible);
            }
        }

        private void HandleTapZonePointerDown(PointerDownEvent evt)
        {
            float now = Time.unscaledTime;
            if (tapCount == 0 || now - firstTapTime > TripleTapWindow)
            {
                tapCount = 1;
                firstTapTime = now;
            }
            else
            {
                tapCount++;
            }

            if (tapCount >= TripleTapCount)
            {
                tapCount = 0;
                SetVisible(!visible);
            }
        }

        private void HandleAdvanceDay()
        {
            if (HasBattle)
            {
                battleController.AdvanceDay();
                return;
            }

            var saveService = new JsonSaveService(SaveLocation.DefaultFilePath);
            var save = saveService.Load();
            DebugDayActions.StartFreshDay(save);
            saveService.Save(save);
            Debug.Log("DebugPanel (menu): started a fresh day. Press Play for a new monster.");
            RefreshMenu();
        }

        private void HandleResetSave()
        {
            if (HasBattle)
            {
                battleController.ResetSave();
                return;
            }

            new JsonSaveService(SaveLocation.DefaultFilePath).Save(new SaveData());
            Debug.Log("DebugPanel (menu): save reset.");
            RefreshMenu();
        }

        private void HandleSeedHistory()
        {
            if (HasBattle)
            {
                battleController.SeedDemoHistory();
                return;
            }

            var saveService = new JsonSaveService(SaveLocation.DefaultFilePath);
            var save = saveService.Load();
            DemoHistoryGenerator.ReplaceHistory(save.history, new SystemDayClock().GetCurrentDayKey(), DemoHistoryGenerator.DefaultSeed);
            saveService.Save(save);
            Debug.Log("DebugPanel (menu): seeded demo history.");
            RefreshMenu();
        }

        private void HandleClearHistory()
        {
            if (HasBattle)
            {
                battleController.ClearHistory();
                return;
            }

            var saveService = new JsonSaveService(SaveLocation.DefaultFilePath);
            var save = saveService.Load();
            save.history.Clear();
            saveService.Save(save);
            Debug.Log("DebugPanel (menu): cleared history.");
            RefreshMenu();
        }

        private void HandleResetTutorial()
        {
            var script = tutorialConfig != null ? tutorialConfig.Script : null;
            if (script == null || script.Steps.Length == 0)
            {
                Debug.LogWarning("DebugPanel: no tutorial config assigned; can't reset the tutorial.", this);
                return;
            }

            new TutorialProgressStore(SaveLocation.TutorialFilePath).Save(
                TutorialProgress.FirstRun(script.Steps[0].Id, tutorialConfig.Version));
            Debug.Log(HasBattle
                ? "DebugPanel: tutorial reset. It starts on the next Main Menu."
                : "DebugPanel (menu): tutorial reset.");
            SetVisible(false);
            if (!HasBattle)
            {
                RefreshMenu();
            }
        }

        private void RefreshMenu()
        {
            if (onRequestGameState != null)
            {
                onRequestGameState.Raise(GameState.MainMenu);
            }
        }

        private void HandleTakeDamageToggled(ChangeEvent<bool> evt)
        {
            battleController.SetTakeDamage(evt.newValue);
        }

        private void SetVisible(bool value)
        {
            visible = value;
            panel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;

            if (value && HasBattle)
            {
                takeDamageToggle.SetValueWithoutNotify(battleController.TakeDamageEnabled);
            }
        }
    }
}
