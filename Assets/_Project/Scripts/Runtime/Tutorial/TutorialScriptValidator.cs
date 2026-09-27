using System.Collections.Generic;
using Spaa.Flow;

namespace Spaa.Tutorial
{
    public static class TutorialScriptValidator
    {
        public const int MaxLineLength = 160;
        public const string DraftMarker = "[DRAFT";

        public static List<string> ValidateStructure(IReadOnlyList<TutorialStepData> steps)
        {
            var errors = new List<string>();
            if (steps == null || steps.Count == 0)
            {
                errors.Add("Script has no steps.");
                return errors;
            }

            var ids = new HashSet<string>();
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step == null || string.IsNullOrEmpty(step.Id))
                {
                    errors.Add($"Step {i} is empty or has no id.");
                    return errors;
                }

                if (!ids.Add(step.Id))
                {
                    errors.Add($"Duplicate step id '{step.Id}'.");
                }
            }

            if (steps[0].Scene != TutorialScene.Menu)
            {
                errors.Add($"First step '{steps[0].Id}' must be in the Menu scene (the tutorial starts on the Main Menu).");
            }

            foreach (var step in steps)
            {
                foreach (var transition in step.Transitions)
                {
                    if (transition == null)
                    {
                        errors.Add($"'{step.Id}' has an empty transition.");
                        continue;
                    }

                    if (!string.IsNullOrEmpty(transition.ToStepId) && !ids.Contains(transition.ToStepId))
                    {
                        errors.Add($"'{step.Id}' {transition.On} goes to missing step '{transition.ToStepId}'.");
                    }
                }

                if (!string.IsNullOrEmpty(step.ResumeStepId) && !ids.Contains(step.ResumeStepId))
                {
                    errors.Add($"'{step.Id}' resumes at missing step '{step.ResumeStepId}'.");
                }

                if (step.Kind == TutorialStepKind.Action && !HasFirstRunTransition(step))
                {
                    errors.Add($"Action step '{step.Id}' has no transition a first-run player can trigger.");
                }
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            CheckReachability(steps, errors);
            CheckBattleResumePoints(steps, errors);
            return errors;
        }

        public static List<string> ValidateCopy(IReadOnlyList<TutorialStepData> steps)
        {
            var errors = new List<string>();
            if (steps == null)
            {
                errors.Add("Script has no steps.");
                return errors;
            }

            int learnMoreCount = 0;
            foreach (var step in steps)
            {
                if (step == null)
                {
                    continue;
                }

                if (step.ShowLearnMore)
                {
                    learnMoreCount++;
                }

                if (step.Lines == null || step.Lines.Length == 0)
                {
                    errors.Add($"'{step.Id}' has no lines.");
                    continue;
                }

                foreach (var line in step.Lines)
                {
                    CheckText(step.Id, line, false, errors);
                }

                CheckText(step.Id, step.Footnote, true, errors);
                CheckText(step.Id, step.NotYetLabel, true, errors);
            }

            if (learnMoreCount != 1)
            {
                errors.Add($"Exactly one step should show Learn more (found {learnMoreCount}).");
            }

            return errors;
        }

        private static void CheckText(string stepId, string text, bool optional, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                if (!optional)
                {
                    errors.Add($"'{stepId}' has an empty line.");
                }

                return;
            }

            if (text.Length > MaxLineLength)
            {
                errors.Add($"'{stepId}' line is {text.Length} chars (max {MaxLineLength}): {text}");
            }

            if (text.Contains(DraftMarker))
            {
                errors.Add($"'{stepId}' still has draft copy: {text}");
            }
        }

        private static bool HasFirstRunTransition(TutorialStepData step)
        {
            foreach (var transition in step.Transitions)
            {
                if (transition != null && !transition.ReplayOnly)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<int> Successors(IReadOnlyList<TutorialStepData> steps, int index)
        {
            var result = new List<int>();
            var step = steps[index];
            bool hasExplicitNext = false;
            foreach (var transition in step.Transitions)
            {
                hasExplicitNext |= transition.On == TutorialSignal.Next;
                result.Add(IndexOf(steps, transition.ToStepId));
            }

            if (step.Kind == TutorialStepKind.Info && !hasExplicitNext)
            {
                result.Add(index + 1 < steps.Count ? index + 1 : -1);
            }

            return result;
        }

        private static void CheckReachability(IReadOnlyList<TutorialStepData> steps, List<string> errors)
        {
            var reached = new bool[steps.Count];
            var queue = new Queue<int>();
            reached[0] = true;
            queue.Enqueue(0);
            while (queue.Count > 0)
            {
                foreach (int next in Successors(steps, queue.Dequeue()))
                {
                    if (next >= 0 && !reached[next])
                    {
                        reached[next] = true;
                        queue.Enqueue(next);
                    }
                }
            }

            var canFinish = new bool[steps.Count];
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < steps.Count; i++)
                {
                    if (canFinish[i])
                    {
                        continue;
                    }

                    foreach (int next in Successors(steps, i))
                    {
                        if (next < 0 || canFinish[next])
                        {
                            canFinish[i] = true;
                            changed = true;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < steps.Count; i++)
            {
                if (!reached[i])
                {
                    errors.Add($"Step '{steps[i].Id}' can't be reached from '{steps[0].Id}'.");
                }

                if (!canFinish[i])
                {
                    errors.Add($"Step '{steps[i].Id}' can never reach the end of the tutorial.");
                }
            }
        }

        private static void CheckBattleResumePoints(IReadOnlyList<TutorialStepData> steps, List<string> errors)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].Scene != TutorialScene.Battle)
                {
                    continue;
                }

                int menuIndex = i - 1;
                while (menuIndex >= 0 && steps[menuIndex].Scene != TutorialScene.Menu)
                {
                    menuIndex--;
                }

                if (menuIndex < 0 || !EntersBattle(steps[menuIndex]))
                {
                    string found = menuIndex < 0 ? "none" : steps[menuIndex].Id;
                    errors.Add($"Battle step '{steps[i].Id}' resumes in the Menu at '{found}', which doesn't lead into Battle. Move Menu-only steps after the Battle steps.");
                }
            }
        }

        private static bool EntersBattle(TutorialStepData step)
        {
            foreach (var transition in step.Transitions)
            {
                if (transition.On == TutorialSignal.StateEntered && transition.State == GameState.Battle)
                {
                    return true;
                }
            }

            return false;
        }

        private static int IndexOf(IReadOnlyList<TutorialStepData> steps, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return -1;
            }

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
