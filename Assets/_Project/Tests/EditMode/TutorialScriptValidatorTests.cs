using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Flow;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialScriptValidatorTests
    {
        private static List<TutorialStepData> Fixture()
        {
            return new List<TutorialStepData>(TutorialTestScript.Steps());
        }

        private static void AssertHasError(List<string> errors, string fragment)
        {
            foreach (var error in errors)
            {
                if (error.Contains(fragment))
                {
                    return;
                }
            }

            Assert.Fail($"Expected an error containing '{fragment}', got: {string.Join(" | ", errors)}");
        }

        private static TutorialStepData Info(string id, TutorialScene scene = TutorialScene.Menu)
        {
            return TutorialStepData.Create(id, TutorialStepKind.Info, scene);
        }

        [Test]
        public void TestFixture_HasValidStructure()
        {
            CollectionAssert.IsEmpty(TutorialScriptValidator.ValidateStructure(Fixture()));
        }

        [Test]
        public void EmptyScript_IsAnError()
        {
            AssertHasError(TutorialScriptValidator.ValidateStructure(new TutorialStepData[0]), "no steps");
            AssertHasError(TutorialScriptValidator.ValidateStructure(null), "no steps");
        }

        [Test]
        public void DuplicateId_IsAnError()
        {
            var steps = Fixture();
            steps.Insert(1, Info("welcome"));

            AssertHasError(TutorialScriptValidator.ValidateStructure(steps), "Duplicate step id 'welcome'");
        }

        [Test]
        public void TransitionToMissingStep_IsAnError()
        {
            var steps = Fixture();
            steps.Insert(1, TutorialStepData.Create("oops", TutorialStepKind.Info, TutorialScene.Menu,
                new[] { new TutorialTransition(TutorialSignal.Next, "no-such-step") }));

            AssertHasError(TutorialScriptValidator.ValidateStructure(steps), "missing step 'no-such-step'");
        }

        [Test]
        public void ResumeAtMissingStep_IsAnError()
        {
            var steps = Fixture();
            steps.Insert(1, TutorialStepData.Create("oops", TutorialStepKind.Info, TutorialScene.Menu, resumeStepId: "gone"));

            AssertHasError(TutorialScriptValidator.ValidateStructure(steps), "resumes at missing step 'gone'");
        }

        [Test]
        public void FirstStepInBattle_IsAnError()
        {
            var steps = Fixture();
            steps.Insert(0, Info("battle-first", TutorialScene.Battle));

            AssertHasError(TutorialScriptValidator.ValidateStructure(steps), "must be in the Menu scene");
        }

        [Test]
        public void ActionWithOnlyReplayTransitions_IsAnError()
        {
            var steps = Fixture();
            steps.Insert(1, TutorialStepData.Create("stuck", TutorialStepKind.Action, TutorialScene.Menu,
                new[] { new TutorialTransition(TutorialSignal.NotYet, "therapy", replayOnly: true) }));

            AssertHasError(TutorialScriptValidator.ValidateStructure(steps), "no transition a first-run player can trigger");
        }

        [Test]
        public void UnreachableStep_IsAnError()
        {
            var steps = Fixture();
            steps.Add(Info("orphan"));

            AssertHasError(TutorialScriptValidator.ValidateStructure(steps), "'orphan' can't be reached");
        }

        [Test]
        public void LoopWithoutExit_IsAnError()
        {
            var steps = new List<TutorialStepData>
            {
                Info("welcome"),
                TutorialStepData.Create("a", TutorialStepKind.Action, TutorialScene.Menu,
                    new[] { new TutorialTransition(TutorialSignal.HabitSaved, "b") }),
                TutorialStepData.Create("b", TutorialStepKind.Action, TutorialScene.Menu,
                    new[] { new TutorialTransition(TutorialSignal.NotYet, "a") })
            };

            var errors = TutorialScriptValidator.ValidateStructure(steps);

            AssertHasError(errors, "'a' can never reach the end");
            AssertHasError(errors, "'welcome' can never reach the end");
        }

        [Test]
        public void MenuStepBeforeBattleSteps_BreaksResume()
        {
            var steps = Fixture();
            var restDay = steps[steps.Count - 1];
            steps.RemoveAt(steps.Count - 1);
            steps.Insert(steps.FindIndex(s => s.Id == "battle-intro"), restDay);

            AssertHasError(TutorialScriptValidator.ValidateStructure(steps), "Battle step 'battle-intro' resumes in the Menu at 'rest-day'");
        }

        [Test]
        public void BattleStepWithNoMenuBeforeIt_BreaksResume()
        {
            var steps = new List<TutorialStepData>
            {
                TutorialStepData.Create("welcome", TutorialStepKind.Info, TutorialScene.Menu,
                    new[] { new TutorialTransition(TutorialSignal.Next, "fight") }),
                Info("fight", TutorialScene.Battle)
            };

            AssertHasError(TutorialScriptValidator.ValidateStructure(steps), "Battle step 'fight' resumes in the Menu at 'welcome'");
        }

        private static List<TutorialStepData> CopyScript()
        {
            return new List<TutorialStepData>
            {
                Info("welcome").WithLines("Hello, hero."),
                Info("therapy").WithLearnMore().WithLines("A line.").WithFootnote("Small print."),
                Info("end").WithLines("Bye!")
            };
        }

        [Test]
        public void CopyScript_IsValid()
        {
            CollectionAssert.IsEmpty(TutorialScriptValidator.ValidateCopy(CopyScript()));
        }

        [Test]
        public void MissingOrEmptyLines_AreErrors()
        {
            var steps = CopyScript();
            steps.Add(Info("silent"));
            steps.Add(Info("blank").WithLines("   "));

            var errors = TutorialScriptValidator.ValidateCopy(steps);

            AssertHasError(errors, "'silent' has no lines");
            AssertHasError(errors, "'blank' has an empty line");
        }

        [Test]
        public void TooLongLine_IsAnError()
        {
            var steps = CopyScript();
            steps.Add(Info("long").WithLines(new string('a', TutorialScriptValidator.MaxLineLength + 1)));

            AssertHasError(TutorialScriptValidator.ValidateCopy(steps), "'long' line is 161 chars");
        }

        [Test]
        public void DraftMarker_IsAnError_InLinesAndFootnotes()
        {
            var steps = CopyScript();
            steps.Add(Info("draft").WithLines("[DRAFT] fill me in"));
            steps.Add(Info("draft-foot").WithLines("Fine.").WithFootnote("[DRAFT – needs final copy]"));

            var errors = TutorialScriptValidator.ValidateCopy(steps);

            AssertHasError(errors, "'draft' still has draft copy");
            AssertHasError(errors, "'draft-foot' still has draft copy");
        }

        [Test]
        public void LearnMore_MustAppearExactlyOnce()
        {
            var none = new List<TutorialStepData> { Info("a").WithLines("x") };
            var twice = CopyScript();
            twice.Add(Info("again").WithLearnMore().WithLines("x"));

            AssertHasError(TutorialScriptValidator.ValidateCopy(none), "found 0");
            AssertHasError(TutorialScriptValidator.ValidateCopy(twice), "found 2");
        }
    }
}
