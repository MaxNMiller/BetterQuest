using System.Collections.Generic;

namespace Spaa.Tutorial
{
    public class TutorialSequencer
    {
        private readonly IReadOnlyList<TutorialStepData> _steps;

        public TutorialSequencer(IReadOnlyList<TutorialStepData> steps)
        {
            _steps = steps ?? new TutorialStepData[0];
        }

        public IReadOnlyList<TutorialStepData> Steps => _steps;

        public int IndexOf(string stepId)
        {
            if (string.IsNullOrEmpty(stepId))
            {
                return -1;
            }

            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i] != null && _steps[i].Id == stepId)
                {
                    return i;
                }
            }

            return -1;
        }

        public TutorialStepData Find(string stepId)
        {
            int index = IndexOf(stepId);
            return index < 0 ? null : _steps[index];
        }

        public TutorialStepData Current(TutorialProgress progress)
        {
            return progress == null || progress.completed ? null : Find(progress.currentStepId);
        }

        public bool Handle(TutorialProgress progress, TutorialEvent evt, bool fallbackActive = false)
        {
            if (progress == null)
            {
                return false;
            }

            if (progress.completed)
            {
                if (evt.Signal != TutorialSignal.Replay || _steps.Count == 0)
                {
                    return false;
                }

                progress.completed = false;
                progress.skipped = false;
                progress.isReplay = true;
                progress.currentStepId = _steps[0].Id;
                return true;
            }

            bool changed = false;
            if (evt.Signal == TutorialSignal.HabitSaved && !progress.firstHabitWritten)
            {
                progress.firstHabitWritten = true;
                changed = true;
            }

            int index = IndexOf(progress.currentStepId);
            if (index < 0)
            {
                GoTo(progress, _steps.Count > 0 ? _steps[0].Id : null);
                return true;
            }

            var step = _steps[index];
            if (evt.Signal == TutorialSignal.Skip)
            {
                if (!TutorialRules.CanSkip(step, progress, fallbackActive))
                {
                    return changed;
                }

                GoTo(progress, null);
                progress.skipped = true;
                return true;
            }

            if (evt.Signal == TutorialSignal.FallbackNext)
            {
                GoTo(progress, NextIdAfter(index));
                return true;
            }

            bool hasExplicitNext = false;
            foreach (var transition in step.Transitions)
            {
                if (transition == null)
                {
                    continue;
                }

                hasExplicitNext |= transition.On == TutorialSignal.Next;
                if (transition.ReplayOnly && !progress.isReplay)
                {
                    continue;
                }

                if (transition.Matches(evt))
                {
                    GoTo(progress, transition.ToStepId);
                    return true;
                }
            }

            if (evt.Signal == TutorialSignal.Next && step.Kind == TutorialStepKind.Info && !hasExplicitNext)
            {
                GoTo(progress, NextIdAfter(index));
                return true;
            }

            return changed;
        }

        private string NextIdAfter(int index)
        {
            return index + 1 < _steps.Count ? _steps[index + 1].Id : null;
        }

        private static void GoTo(TutorialProgress progress, string stepId)
        {
            if (string.IsNullOrEmpty(stepId))
            {
                progress.completed = true;
                progress.currentStepId = string.Empty;
                return;
            }

            progress.currentStepId = stepId;
        }
    }
}
