using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Battle;
using Spaa.DayCycle;
using Spaa.Elements;
using Spaa.Events;
using Spaa.Flow;
using Spaa.Save;

namespace Spaa.Tutorial
{
    public class TutorialRunner : MonoBehaviour
    {
        private const string HelpButtonName = "tutorial-help-button";
        private const string HabitsButtonName = "habit-menu-button";

        [SerializeField] private TutorialConfigSO config;
        [SerializeField] private TutorialOverlayView overlay;
        [SerializeField] private TutorialScene scene;
        [Tooltip("Menu only: the document holding the Main Menu's ? button.")]
        [SerializeField] private UIDocument menuDocument;
        [Tooltip("Display names for the Play step's missing-pillar line. Indexed by Element.")]
        [SerializeField] private ElementDefinition[] elementDefinitions = new ElementDefinition[0];
        [Tooltip("Battle only: wait for the entrance scrim before the first step appears.")]
        [SerializeField] private float battleShowDelay = 0.6f;

        [Header("Channels (listen only)")]
        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [SerializeField] private VoidEventChannelSO onHabitEditorOpened;
        [SerializeField] private VoidEventChannelSO onHabitEditorClosed;
        [SerializeField] private VoidEventChannelSO onHabitSaved;
        [SerializeField] private AttackResultEventChannelSO onAttackResolved;
        [SerializeField] private VoidEventChannelSO onMonsterDefeated;

        private TutorialProgressStore _store;
        private ISaveService _saveService;
        private IDayClock _dayClock;
        private TutorialSequencer _sequencer;
        private TutorialSession _session;
        private Button _helpButton;
        private bool _started;
        private bool _dirty;
        private bool _promptOpen;
        private bool _loggedCorrupt;
        private bool _tipPending;
        private GameState _tipState;
        private TutorialTipData _tip;
        private float _showAfter;

        public TutorialSession Session => _session;

        private TutorialScriptSO Script => config != null ? config.Script : null;

        private void OnEnable()
        {
            if (config == null || overlay == null)
            {
                Debug.LogWarning("TutorialRunner: config or overlay isn't assigned; the tutorial is off.", this);
                return;
            }

            _store = new TutorialProgressStore(SaveLocation.TutorialFilePath);
            _saveService = new JsonSaveService(SaveLocation.DefaultFilePath);
            _dayClock = new SystemDayClock();
            _sequencer = new TutorialSequencer(Script != null ? Script.Steps : null);
            _session = new TutorialSession(_sequencer, _store, scene, config.Enabled, config.Version, config.ReplayOnVersionBump);

            Register(onHabitEditorOpened, HandleEditorOpened);
            Register(onHabitEditorClosed, HandleEditorClosed);
            Register(onHabitSaved, HandleHabitSaved);
            Register(onMonsterDefeated, HandleMonsterDefeated);
            if (onRequestGameState != null)
            {
                onRequestGameState.RegisterListener(HandleStateRequested);
            }

            if (onAttackResolved != null)
            {
                onAttackResolved.RegisterListener(HandleAttackResolved);
            }

            overlay.NextClicked += HandleNext;
            overlay.NotYetClicked += HandleNotYet;
            overlay.SkipClicked += HandleSkip;
            overlay.LearnMoreClicked += HandleLearnMore;
            overlay.ReplayConfirmed += HandleReplayConfirmed;
            overlay.ReplayDeclined += HandleReplayDeclined;
            overlay.FallbackEntered += HandleFallback;

            if (scene == TutorialScene.Battle)
            {
                _showAfter = Time.unscaledTime + battleShowDelay;
                Begin(_session.State);
            }

            if (_started)
            {
                HookHelpButton();
            }
        }

        private void Start()
        {
            _started = true;
            if (_session != null)
            {
                HookHelpButton();
            }
        }

        private void OnDisable()
        {
            if (_session == null)
            {
                return;
            }

            Unregister(onHabitEditorOpened, HandleEditorOpened);
            Unregister(onHabitEditorClosed, HandleEditorClosed);
            Unregister(onHabitSaved, HandleHabitSaved);
            Unregister(onMonsterDefeated, HandleMonsterDefeated);
            if (onRequestGameState != null)
            {
                onRequestGameState.UnregisterListener(HandleStateRequested);
            }

            if (onAttackResolved != null)
            {
                onAttackResolved.UnregisterListener(HandleAttackResolved);
            }

            overlay.NextClicked -= HandleNext;
            overlay.NotYetClicked -= HandleNotYet;
            overlay.SkipClicked -= HandleSkip;
            overlay.LearnMoreClicked -= HandleLearnMore;
            overlay.ReplayConfirmed -= HandleReplayConfirmed;
            overlay.ReplayDeclined -= HandleReplayDeclined;
            overlay.FallbackEntered -= HandleFallback;
            UnhookHelpButton();
            _session = null;
        }

        private void Update()
        {
            if (!_dirty || Time.unscaledTime < _showAfter)
            {
                return;
            }

            _dirty = false;
            Refresh();
        }

        private void HandleStateRequested(GameState state)
        {
            _tip = null;
            _promptOpen = false;
            bool quietStart = false;
            if (scene == TutorialScene.Menu && state == GameState.MainMenu && !_session.IsActive)
            {
                quietStart = Begin(state) == TutorialStartDecision.MarkCompleteSilently;
            }
            else
            {
                _session.StateEntered(state);
            }

            _tipPending = !_session.IsActive && !quietStart;
            _tipState = state;
            _dirty = true;
        }

        private TutorialStartDecision Begin(GameState state)
        {
            var decision = _session.Begin(_saveService.Load(), state);
            if (_store.LastLoadWasCorrupt && !_loggedCorrupt)
            {
                _loggedCorrupt = true;
                Debug.LogWarning($"Tutorial: {SaveLocation.TutorialFileName} couldn't be read; treating the tutorial as finished.", this);
            }

            _dirty = true;
            return decision;
        }

        private void Refresh()
        {
            if (_session == null)
            {
                return;
            }

            if (!_session.IsActive)
            {
                if (_tipPending)
                {
                    _tipPending = false;
                    var save = _saveService.Load();
                    bool hasBattled = save.monsterSeed != 0 || (save.history != null && save.history.Count > 0);
                    _tip = _session.TakeTip(Script != null ? Script.Tips : null, _tipState, hasBattled);
                }

                if (_tip != null)
                {
                    overlay.Show(_tip.Callout, TutorialOverlayModel.ForTip(Script.TipGotItLabel), null);
                }
                else if (!_promptOpen)
                {
                    overlay.Hide();
                }

                return;
            }

            _tip = null;
            _promptOpen = false;
            var step = _session.VisibleStep;
            if (step == null)
            {
                overlay.Hide();
                return;
            }

            string[] lines = null;
            string target = null;
            if (step.Id == TutorialSession.PlayStepId)
            {
                var save = _saveService.Load();
                string today = _dayClock.GetCurrentDayKey();
                var variant = TutorialPlayVariantResolver.Resolve(save, today);
                if (_session.ApplyPlayVariant(variant))
                {
                    Refresh();
                    return;
                }

                if (variant == TutorialPlayVariant.MissingHabits && Script != null)
                {
                    var missing = TodayStatusResolver.Resolve(save, today).MissingElements;
                    lines = new[] { string.Format(Script.PlayMissingHabitsLine, PillarNames(missing)) };
                    target = HabitsButtonName;
                }
                else if (variant == TutorialPlayVariant.LevelUpPending && Script != null)
                {
                    lines = new[] { Script.PlayClaimRewardLine };
                }
            }

            var model = TutorialOverlayModel.Build(_sequencer, step, _session.Progress, _session.Fallback);
            overlay.Show(step, model, lines, target);
        }

        private string PillarNames(IReadOnlyList<Element> elements)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < elements.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(i == elements.Count - 1 ? " and " : ", ");
                }

                builder.Append(PillarName(elements[i]));
            }

            return builder.ToString();
        }

        private string PillarName(Element element)
        {
            int index = (int)element;
            if (elementDefinitions != null && index < elementDefinitions.Length && elementDefinitions[index] != null
                && !string.IsNullOrEmpty(elementDefinitions[index].DisplayName))
            {
                return elementDefinitions[index].DisplayName;
            }

            return element.ToString();
        }

        private void HookHelpButton()
        {
            if (scene != TutorialScene.Menu || _helpButton != null)
            {
                return;
            }

            var root = menuDocument != null ? menuDocument.rootVisualElement : null;
            _helpButton = root != null ? root.Q<Button>(HelpButtonName) : null;
            if (_helpButton == null)
            {
                Debug.LogWarning($"TutorialRunner: '{HelpButtonName}' not found; the tutorial can't be replayed from the menu.", this);
                return;
            }

            if (!config.Enabled)
            {
                _helpButton.style.display = DisplayStyle.None;
                return;
            }

            _helpButton.clicked += HandleHelpClicked;
        }

        private void UnhookHelpButton()
        {
            if (_helpButton != null)
            {
                _helpButton.clicked -= HandleHelpClicked;
                _helpButton = null;
            }
        }

        private void HandleHelpClicked()
        {
            if (_session == null || _session.IsActive || Script == null)
            {
                return;
            }

            _tip = null;
            _promptOpen = true;
            overlay.ShowReplayPrompt(Script.ReplayPrompt);
        }

        private void HandleReplayConfirmed()
        {
            _promptOpen = false;
            _session.Replay();
            _dirty = true;
        }

        private void HandleReplayDeclined()
        {
            _promptOpen = false;
            _dirty = true;
        }

        private void HandleNext()
        {
            if (_tip != null)
            {
                _tip = null;
                overlay.Hide();
                return;
            }

            Forward(TutorialSignal.Next);
        }

        private void HandleLearnMore()
        {
            string url = config.LearnMoreUrl;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            Debug.Log($"Tutorial: opening {url}", this);
            Application.OpenURL(url);
        }

        private void HandleFallback()
        {
            _session.EnterFallback();
            _dirty = true;
        }

        private void HandleNotYet() => Forward(TutorialSignal.NotYet);
        private void HandleSkip() => Forward(TutorialSignal.Skip);
        private void HandleEditorOpened() => Forward(TutorialSignal.HabitEditorOpened);
        private void HandleEditorClosed() => Forward(TutorialSignal.HabitEditorClosed);
        private void HandleHabitSaved() => Forward(TutorialSignal.HabitSaved);
        private void HandleMonsterDefeated() => Forward(TutorialSignal.MonsterDefeated);
        private void HandleAttackResolved(CommandResult result) => Forward(TutorialSignal.AttackResolved);

        private void Forward(TutorialSignal signal)
        {
            if (_session.Signal(signal))
            {
                _dirty = true;
            }
        }

        private static void Register(VoidEventChannelSO channel, System.Action handler)
        {
            if (channel != null)
            {
                channel.RegisterListener(handler);
            }
        }

        private static void Unregister(VoidEventChannelSO channel, System.Action handler)
        {
            if (channel != null)
            {
                channel.UnregisterListener(handler);
            }
        }
    }
}
