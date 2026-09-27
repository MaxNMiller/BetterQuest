using System.Collections.Generic;
using Spaa.Flow;
using Spaa.Save;

namespace Spaa.Tutorial
{
    public class TutorialSession
    {
        public const string PlayStepId = "play";

        private readonly TutorialSequencer _sequencer;
        private readonly ITutorialProgressStore _store;
        private readonly TutorialScene _scene;
        private readonly bool _enabled;
        private readonly int _version;
        private readonly bool _replayOnVersionBump;
        private GameState _stepState;

        public TutorialSession(TutorialSequencer sequencer, ITutorialProgressStore store, TutorialScene scene,
            bool enabled, int version, bool replayOnVersionBump)
        {
            _sequencer = sequencer ?? new TutorialSequencer(null);
            _store = store;
            _scene = scene;
            _enabled = enabled;
            _version = version;
            _replayOnVersionBump = replayOnVersionBump;
            State = HomeState(scene);
            _stepState = State;
        }

        public TutorialProgress Progress { get; private set; }
        public GameState State { get; private set; }
        public bool Fallback { get; private set; }
        public bool IsActive => Progress != null && !Progress.completed;

        public TutorialStepData VisibleStep
        {
            get
            {
                var step = _sequencer.Current(Progress);
                return step == null || step.Scene != _scene || State != _stepState ? null : step;
            }
        }

        public static GameState HomeState(TutorialScene scene)
        {
            return scene == TutorialScene.Battle ? GameState.Battle : GameState.MainMenu;
        }

        public TutorialStartDecision Begin(SaveData save, GameState state)
        {
            State = state;
            if (!_enabled)
            {
                return TutorialStartDecision.Disabled;
            }

            if (IsActive)
            {
                return TutorialStartDecision.None;
            }

            var loaded = _store != null ? _store.Load() : null;
            var decision = TutorialEligibility.Evaluate(loaded, save, true, _version, _replayOnVersionBump);
            Progress = loaded;
            switch (decision)
            {
                case TutorialStartDecision.Start:
                    if (_scene != TutorialScene.Menu || _sequencer.Steps.Count == 0)
                    {
                        return TutorialStartDecision.None;
                    }

                    Progress = TutorialProgress.FirstRun(_sequencer.Steps[0].Id, _version);
                    break;

                case TutorialStartDecision.MarkCompleteSilently:
                    Progress = TutorialProgress.CompletedSilently(_version);
                    break;

                case TutorialStartDecision.Replay:
                    if (_scene != TutorialScene.Menu)
                    {
                        return TutorialStartDecision.None;
                    }

                    _sequencer.Handle(Progress, TutorialEvent.Of(TutorialSignal.Replay));
                    Progress.version = _version;
                    break;

                case TutorialStartDecision.Resume:
                    var start = TutorialResume.ResolveStart(_sequencer, Progress, _scene);
                    if (start == null)
                    {
                        return decision;
                    }

                    Progress.currentStepId = start.Id;
                    break;

                default:
                    return decision;
            }

            StepChanged();
            _stepState = HomeState(_scene);
            Persist();
            return decision;
        }

        public bool StateEntered(GameState state)
        {
            State = state;
            if (!IsActive)
            {
                return false;
            }

            return Apply(TutorialEvent.Entered(state));
        }

        public bool Signal(TutorialSignal signal)
        {
            if (!IsActive)
            {
                return false;
            }

            if (signal == TutorialSignal.Next && Fallback)
            {
                signal = TutorialSignal.FallbackNext;
            }

            return Apply(TutorialEvent.Of(signal));
        }

        public void EnterFallback()
        {
            if (IsActive)
            {
                Fallback = true;
            }
        }

        public bool ApplyPlayVariant(TutorialPlayVariant variant)
        {
            var step = VisibleStep;
            if (step == null || step.Id != PlayStepId || variant != TutorialPlayVariant.Defeated)
            {
                return false;
            }

            return Signal(TutorialSignal.PlayUnavailable);
        }

        public bool Replay()
        {
            if (!_enabled || _scene != TutorialScene.Menu || IsActive || _sequencer.Steps.Count == 0)
            {
                return false;
            }

            if (Progress == null)
            {
                Progress = TutorialProgress.CompletedSilently(_version);
            }

            _sequencer.Handle(Progress, TutorialEvent.Of(TutorialSignal.Replay));
            Progress.version = _version;
            StepChanged();
            Persist();
            return true;
        }

        public TutorialTipData TakeTip(IReadOnlyList<TutorialTipData> tips, GameState entered, bool hasBattled)
        {
            if (!_enabled)
            {
                return null;
            }

            var tip = TutorialTips.Pick(tips, Progress, entered, _scene, hasBattled);
            if (tip != null && TutorialTips.MarkSeen(Progress, tip.Id))
            {
                Persist();
            }

            return tip;
        }

        private bool Apply(TutorialEvent evt)
        {
            string before = Progress.currentStepId;
            bool wasCompleted = Progress.completed;
            if (!_sequencer.Handle(Progress, evt, Fallback))
            {
                return false;
            }

            if (before != Progress.currentStepId || wasCompleted != Progress.completed)
            {
                StepChanged();
            }

            Persist();
            return true;
        }

        private void StepChanged()
        {
            Fallback = false;
            _stepState = State;
        }

        private void Persist()
        {
            if (_enabled && _store != null && Progress != null)
            {
                _store.Save(Progress);
            }
        }
    }
}
