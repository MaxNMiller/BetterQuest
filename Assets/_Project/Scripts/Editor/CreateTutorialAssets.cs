using System.IO;
using UnityEditor;
using UnityEngine;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Events;
using Spaa.Flow;
using Spaa.Tutorial;
using Spaa.UI;

namespace Spaa.EditorTools
{
    public static class CreateTutorialAssets
    {
        public const string ScriptPath = "Assets/_Project/Data/Config/TutorialScript.asset";
        public const string ConfigPath = "Assets/_Project/Data/Config/TutorialConfig.asset";
        private const string FaceLibraryPath = "Assets/_Project/Data/Monsters/GloobFaceLibrary.asset";
        private const string MotionSettingsPath = "Assets/_Project/Data/Config/UiMotionSettings.asset";
        public const string OnHabitSavedPath = "Assets/_Project/Data/Events/OnHabitSaved.asset";
        public const string OnHabitEditorOpenedPath = "Assets/_Project/Data/Events/OnHabitEditorOpened.asset";
        public const string OnHabitEditorClosedPath = "Assets/_Project/Data/Events/OnHabitEditorClosed.asset";

        public const string ThinkSmartDescription =
            "THINK+SMART is a self-guided metabolic wellness program designed to improve mental health and brain function through nutrition and daily habits.";

        [MenuItem("Tools/Spaa/Tutorial/Create Assets")]
        public static void Create()
        {
            var script = AssetDatabase.LoadAssetAtPath<TutorialScriptSO>(ScriptPath);
            if (script == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScriptPath));
                script = ScriptableObject.CreateInstance<TutorialScriptSO>();
                script.EditorSetSteps(DefaultSteps());
                AssetDatabase.CreateAsset(script, ScriptPath);
                Debug.Log($"Created TutorialScriptSO at {ScriptPath}");
            }

            if (script.Tips == null || script.Tips.Length == 0)
            {
                script.EditorSetTips(DefaultTips());
                EditorUtility.SetDirty(script);
                Debug.Log("Added the default tutorial tips.");
            }

            if (AssetDatabase.LoadAssetAtPath<TutorialConfigSO>(ConfigPath) == null)
            {
                var config = ScriptableObject.CreateInstance<TutorialConfigSO>();
                AssetDatabase.CreateAsset(config, ConfigPath);
                var serialized = new SerializedObject(config);
                serialized.FindProperty("script").objectReferenceValue = script;
                serialized.FindProperty("faceLibrary").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<MonsterFaceLibrary>(FaceLibraryPath);
                serialized.FindProperty("motionSettings").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<UiMotionSettingsSO>(MotionSettingsPath);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"Created TutorialConfigSO at {ConfigPath}");
            }

            CreateChannelIfMissing(OnHabitSavedPath);
            CreateChannelIfMissing(OnHabitEditorOpenedPath);
            CreateChannelIfMissing(OnHabitEditorClosedPath);

            AssetDatabase.SaveAssets();
        }

        private static void CreateChannelIfMissing(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<VoidEventChannelSO>(path) != null)
            {
                return;
            }

            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<VoidEventChannelSO>(), path);
            Debug.Log($"Created VoidEventChannelSO at {path}");
        }

        public static TutorialStepData[] DefaultSteps()
        {
            const TutorialScene Menu = TutorialScene.Menu;
            const TutorialScene Battle = TutorialScene.Battle;

            return new[]
            {
                Info("welcome", Menu).WithLines(
                    "Well met, hero! I am Wombino the Wise.",
                    "In Better Quest you win battles by finishing real-life habits. Let me show you around."),

                Info("therapy-1", Menu).WithLearnMore().WithLines(
                    ThinkSmartDescription,
                    "Tap Learn more to read about the program."),

                Info("therapy-2", Menu).WithLines(
                    "Better Quest turns those daily habits into a battle.",
                    "Plan your habits, do them in real life, then log them here to attack!")
                    .WithFootnote("Not medical advice. Follow your care team's guidance."),

                Info("elements", Menu).WithElements().WithLines(
                    "Every habit belongs to one of five pillars:",
                    "Rest: sleep and quiet time. Self Care: looking after your body.",
                    "Food: what and when you eat. Rebuild: friends, feelings and reflection. Move: getting active."),

                Info("foe", Menu, "today-plaque").WithLines(
                    "Each day a new monster appears.",
                    "It's weak to one pillar: habits from that pillar hit twice as hard."),

                Step("open-journal", TutorialStepKind.Action, Menu, "habit-menu-button", locked: true,
                        new TutorialTransition(TutorialSignal.StateEntered, "journal", GameState.HabitMenu))
                    .WithLines("Let's write your habits. Tap Habits."),

                Step("journal", TutorialStepKind.Info, Menu, "habit-list-scroll", locked: true)
                    .WithResume("open-journal")
                    .WithLines(
                        "These starter habits fill every pillar.",
                        "Tap any card later to rename it or change how often you do it."),

                Step("add-habit", TutorialStepKind.Action, Menu, "habit-add-button", locked: true,
                        new TutorialTransition(TutorialSignal.HabitEditorOpened, "write-name"),
                        new TutorialTransition(TutorialSignal.NotYet, "back-to-menu", replayOnly: true))
                    .WithResume("open-journal")
                    .WithNotYetLabel("Skip for now")
                    .WithLines(
                        "Now add one habit from your own plan. Tap New Habit.",
                        "Every hero starts with one!"),

                Step("write-name", TutorialStepKind.Action, Menu, "habit-name-field", locked: true,
                        new TutorialTransition(TutorialSignal.HabitSaved, "habit-saved"),
                        new TutorialTransition(TutorialSignal.HabitEditorClosed, "add-habit"),
                        new TutorialTransition(TutorialSignal.NotYet, "back-to-menu", replayOnly: true))
                    .WithHole("habit-edit-card", TutorialDock.Top)
                    .WithResume("open-journal")
                    .WithNotYetLabel("Skip for now")
                    .WithLines(
                        "Write it the way you'd say it, like \"Walk after lunch\".",
                        "Pick its pillar and how many times a day, then tap Save Habit."),

                Info("habit-saved", Menu, "habit-list-scroll").WithFace(Element.Move).WithResume("play").WithLines(
                    "Wonderful! That habit is now one of your attacks.",
                    "Every pillar needs at least one habit to battle. You can add more any time."),

                Step("back-to-menu", TutorialStepKind.Action, Menu, "habit-menu-back-button", locked: false,
                        new TutorialTransition(TutorialSignal.StateEntered, "play", GameState.MainMenu))
                    .WithResume("play")
                    .WithLines("Now let's meet today's monster. Tap Back."),

                Step("play", TutorialStepKind.Action, Menu, "play-button", locked: false,
                        new TutorialTransition(TutorialSignal.StateEntered, "battle-intro", GameState.Battle),
                        new TutorialTransition(TutorialSignal.PlayUnavailable, "rest-day"))
                    .WithLines("Tap Battle! when you're ready."),

                Info("battle-intro", Battle, "nameplate").WithLines(
                    "Here's today's monster and its HP bar.",
                    "Finish about 70% of today's habits to knock it out."),

                Info("weakness", Battle, "weakness-chip").WithLines(
                    "This chip shows its weakness. Habits from that pillar deal double damage!"),

                Info("attack-sheet", Battle, "attack-sheet").WithLines(
                    "Each button is one of your habits.",
                    "Only tap one after you've really done it. That's the hero's honour code!"),

                Step("first-attack", TutorialStepKind.Action, Battle, "attack-sheet", locked: false,
                        new TutorialTransition(TutorialSignal.AttackResolved, "hit"),
                        new TutorialTransition(TutorialSignal.NotYet, "come-back"))
                    .WithNotYetLabel("Not yet")
                    .WithLines(
                        "Done one of these today already? Tap it to attack!",
                        "If not, tap Not yet."),

                Step("hit", TutorialStepKind.Info, Battle, "hp-bar-track", locked: false,
                        new TutorialTransition(TutorialSignal.Next, "wrap-up"))
                    .WithFace(Element.Food)
                    .WithLines("Direct hit! Keep logging habits through the day to wear it down."),

                Info("come-back", Battle).WithLines(
                    "No rush. Come back after your first habit.",
                    "The monster will wait for you."),

                Step("wrap-up", TutorialStepKind.Info, Battle, null, locked: false,
                        new TutorialTransition(TutorialSignal.Next, string.Empty))
                    .WithFace(Element.Rest, MonsterExpression.Hurt)
                    .WithLines(
                        "If the monster is still standing at midnight, it hits back gently. There's no game over.",
                        "Beat it to level up a pillar. Good luck, hero!"),

                Step("rest-day", TutorialStepKind.Info, Menu, "today-plaque", locked: false,
                        new TutorialTransition(TutorialSignal.Next, string.Empty))
                    .WithFace(Element.Move)
                    .WithLines(
                        "Today's monster is already beaten! Your next one arrives tomorrow.",
                        "Come back then and put your habits to work.")
            };
        }

        [MenuItem("Tools/Spaa/Tutorial/Reset Script To Defaults")]
        public static void ResetScriptToDefaults()
        {
            var script = AssetDatabase.LoadAssetAtPath<TutorialScriptSO>(ScriptPath);
            if (script == null)
            {
                Create();
                return;
            }

            script.EditorSetSteps(DefaultSteps());
            script.EditorSetTips(DefaultTips());
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
            Debug.Log($"Reset {ScriptPath} to the default steps and tips.");
        }

        public static TutorialTipData[] DefaultTips()
        {
            var levelUp = TutorialTipData.Create("tip-levelup", GameState.LevelUp, TutorialScene.Battle, "level-up-featured");
            levelUp.Callout.WithFace(Element.Move).WithDock(TutorialDock.Bottom).WithLines(
                "Victory! Pick a pillar to level up.",
                "The recommended one is today's weakness. Stronger pillars hit harder.");

            var reminder = TutorialTipData.Create("tip-reminder", GameState.SoftReminder, TutorialScene.Battle, "adjust-habits-button");
            reminder.Callout.WithDock(TutorialDock.Bottom).WithLines(
                "Missing a day is okay. Every hero rests.",
                "If your plan felt too heavy, tap Adjust Habits to lighten it.");

            var progress = TutorialTipData.Create("tip-progress", GameState.MainMenu, TutorialScene.Menu, "dashboard-button",
                afterFirstBattle: true);
            progress.Callout.WithDock(TutorialDock.Top).WithLines(
                "Tap Progress to see your Adventure Log.",
                "It shows your streaks and which pillars need some love.");

            return new[] { levelUp, reminder, progress };
        }

        private static TutorialStepData Info(string id, TutorialScene scene, string target = null)
        {
            return TutorialStepData.Create(id, TutorialStepKind.Info, scene, targetName: target);
        }

        private static TutorialStepData Step(string id, TutorialStepKind kind, TutorialScene scene, string target,
            bool locked, params TutorialTransition[] transitions)
        {
            return TutorialStepData.Create(id, kind, scene, transitions, lockSkipUntilFirstHabit: locked, targetName: target);
        }
    }
}
