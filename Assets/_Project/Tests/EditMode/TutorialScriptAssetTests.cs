using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Flow;
using Spaa.Tutorial;
using UnityEditor;
using UnityEngine.UIElements;

namespace Spaa.Tests
{
    public class TutorialScriptAssetTests
    {
        private const string ConfigPath = "Assets/_Project/Data/Config/TutorialConfig.asset";
        private const string ScriptPath = "Assets/_Project/Data/Config/TutorialScript.asset";
        private const string MenuUxml = "Assets/_Project/UI/UXML/MenuFlow.uxml";
        private const string BattleHudUxml = "Assets/_Project/UI/UXML/BattleHud.uxml";
        private const string OverlaysUxml = "Assets/_Project/UI/UXML/Overlays.uxml";
        private const string ThinkSmartDescription =
            "THINK+SMART is a self-guided metabolic wellness program designed to improve mental health and brain function through nutrition and daily habits.";

        private TutorialConfigSO config;
        private TutorialScriptSO script;

        [SetUp]
        public void SetUp()
        {
            config = AssetDatabase.LoadAssetAtPath<TutorialConfigSO>(ConfigPath);
            script = AssetDatabase.LoadAssetAtPath<TutorialScriptSO>(ScriptPath);
            Assert.IsNotNull(config, $"Missing {ConfigPath}. Run Tools/Spaa/Tutorial/Create Assets.");
            Assert.IsNotNull(script, $"Missing {ScriptPath}. Run Tools/Spaa/Tutorial/Create Assets.");
        }

        [Test]
        public void Config_IsWiredWithApprovedValues()
        {
            Assert.AreSame(script, config.Script);
            Assert.IsNotNull(config.FaceLibrary, "faceLibrary");
            Assert.IsNotNull(config.MotionSettings, "motionSettings");
            Assert.IsTrue(config.Enabled);
            Assert.GreaterOrEqual(config.Version, 1);
            Assert.AreEqual("Wombino the Wise", config.GuideName);
            Assert.IsNotNull(config.Portrait, "portrait (Art/WombinoTheWiseCircle.png)");
            Assert.AreEqual("https://www.metabolicmind.org/thinksmart/", config.LearnMoreUrl);
        }

        [Test]
        public void Script_HasValidStructureAndCopy()
        {
            var errors = TutorialScriptValidator.ValidateStructure(script.Steps);
            errors.AddRange(TutorialScriptValidator.ValidateCopy(script.Steps));

            CollectionAssert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void LearnMoreStep_ShowsApprovedDescriptionVerbatim()
        {
            foreach (var step in script.Steps)
            {
                if (step.ShowLearnMore)
                {
                    CollectionAssert.Contains(step.Lines, ThinkSmartDescription);
                    return;
                }
            }

            Assert.Fail("No Learn more step.");
        }

        [Test]
        public void MedicalDisclaimer_IsPresent()
        {
            foreach (var step in script.Steps)
            {
                if (!string.IsNullOrEmpty(step.Footnote) && step.Footnote.Contains("Not medical advice"))
                {
                    return;
                }
            }

            Assert.Fail("No step carries the 'Not medical advice' footnote.");
        }

        [Test]
        public void EveryTarget_ExistsInItsScenesUxml()
        {
            var menu = Load(MenuUxml);
            var battle = new[] { Load(BattleHudUxml), Load(OverlaysUxml) };
            var missing = new List<string>();

            foreach (var step in script.Steps)
            {
                foreach (var name in new[] { step.TargetName, step.HoleTargetName })
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    bool found = step.Scene == TutorialScene.Menu
                        ? menu.Q(name) != null
                        : battle[0].Q(name) != null || battle[1].Q(name) != null;
                    if (!found)
                    {
                        missing.Add($"{step.Id} → '{name}' ({step.Scene})");
                    }
                }
            }

            CollectionAssert.IsEmpty(missing, "Tutorial targets missing from UXML:\n" + string.Join("\n", missing));
        }

        [Test]
        public void EveryStepFace_ResolvesToGloobTextures()
        {
            foreach (var step in script.Steps)
            {
                var face = config.FaceLibrary.GetFace(step.Expression, step.FaceElement);
                Assert.IsNotNull(face.Eye, $"{step.Id}: no eye texture for {step.Expression}/{step.FaceElement}");
                Assert.IsNotNull(face.Mouth, $"{step.Id}: no mouth texture for {step.Expression}/{step.FaceElement}");
            }
        }

        [Test]
        public void FirstHabitSteps_OfferNoEscapeOnFirstRun()
        {
            bool sawHabitSaved = false;
            foreach (var step in script.Steps)
            {
                foreach (var transition in step.Transitions)
                {
                    if (transition.On == TutorialSignal.HabitSaved)
                    {
                        sawHabitSaved = true;
                        Assert.IsTrue(step.LockSkipUntilFirstHabit, $"{step.Id} must lock Skip until a habit is saved.");
                    }

                    if (step.LockSkipUntilFirstHabit && transition.On == TutorialSignal.NotYet)
                    {
                        Assert.IsTrue(transition.ReplayOnly, $"{step.Id}: NotYet must be replay-only on a required step.");
                    }
                }
            }

            Assert.IsTrue(sawHabitSaved, "No step waits for a saved habit.");
        }

        [Test]
        public void PlayVariantLines_ArePresent()
        {
            StringAssert.Contains("{0}", script.PlayMissingHabitsLine);
            Assert.IsFalse(string.IsNullOrWhiteSpace(script.PlayClaimRewardLine));
            Assert.IsFalse(string.IsNullOrWhiteSpace(script.ReplayPrompt));
        }

        [Test]
        public void MainMenuPlayStep_LeadsIntoBattle()
        {
            var sequencer = new TutorialSequencer(script.Steps);
            var progress = TutorialProgress.FirstRun("play", config.Version);

            Assert.IsNotNull(sequencer.Find("play"), "The runner looks up the 'play' step by id for its variants.");
            Assert.IsTrue(sequencer.Handle(progress, TutorialEvent.Entered(GameState.Battle)));
            Assert.AreEqual(TutorialScene.Battle, sequencer.Find(progress.currentStepId).Scene);
        }

        [Test]
        public void HabitMenuSteps_ResumeOnTheMainMenu()
        {
            var menu = Load(MenuUxml);
            var habitPanel = menu.Q("habit-menu-panel");
            var mainPanel = menu.Q("main-menu-panel");
            var sequencer = new TutorialSequencer(script.Steps);
            var stranded = new List<string>();

            foreach (var step in script.Steps)
            {
                if (step.Scene != TutorialScene.Menu || string.IsNullOrEmpty(step.TargetName) || habitPanel.Q(step.TargetName) == null)
                {
                    continue;
                }

                var resume = sequencer.Find(step.ResumeStepId);
                if (resume == null || string.IsNullOrEmpty(resume.TargetName) || mainPanel.Q(resume.TargetName) == null)
                {
                    stranded.Add(step.Id);
                }
            }

            CollectionAssert.IsEmpty(stranded, "Habit Menu steps without a Main Menu resume point: " + string.Join(", ", stranded));
        }

        [Test]
        public void Tips_AreCompleteAndTargetRealElements()
        {
            var menu = Load(MenuUxml);
            var battle = new[] { Load(BattleHudUxml), Load(OverlaysUxml) };
            var ids = new HashSet<string>();

            Assert.AreEqual(3, script.Tips.Length);
            foreach (var tip in script.Tips)
            {
                Assert.IsTrue(ids.Add(tip.Id), $"Duplicate tip id {tip.Id}");
                Assert.IsNotNull(tip.Callout, tip.Id);
                Assert.IsNotEmpty(tip.Callout.Lines, tip.Id);
                foreach (var line in tip.Callout.Lines)
                {
                    Assert.LessOrEqual(line.Length, TutorialScriptValidator.MaxLineLength, tip.Id);
                }

                string target = tip.Callout.TargetName;
                bool found = tip.Scene == TutorialScene.Menu
                    ? menu.Q(target) != null
                    : battle[0].Q(target) != null || battle[1].Q(target) != null;
                Assert.IsTrue(found, $"{tip.Id} → '{target}' missing from its scene's UXML");
                Assert.IsNotNull(config.FaceLibrary.GetFace(tip.Callout.Expression, tip.Callout.FaceElement).Eye, tip.Id);
            }

            CollectionAssert.IsSubsetOf(new[] { "tip-levelup", "tip-reminder", "tip-progress" }, ids);
            Assert.IsFalse(string.IsNullOrWhiteSpace(script.TipGotItLabel));
        }

        private static VisualElement Load(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Assert.IsNotNull(asset, path);
            return asset.CloneTree();
        }
    }
}
