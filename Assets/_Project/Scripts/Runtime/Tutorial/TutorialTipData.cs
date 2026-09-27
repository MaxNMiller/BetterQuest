using System;
using UnityEngine;
using Spaa.Flow;

namespace Spaa.Tutorial
{
    [Serializable]
    public class TutorialTipData
    {
        [Tooltip("Stored in TutorialProgress.seenTips once shown.")]
        [SerializeField] private string id;
        [SerializeField] private GameState trigger;
        [SerializeField] private TutorialScene scene;
        [Tooltip("Only after the player has fought at least once (e.g. the Adventure Log tip).")]
        [SerializeField] private bool afterFirstBattle;
        [Tooltip("Target, lines and face. Kind and transitions are ignored: tips always close with Got it.")]
        [SerializeField] private TutorialStepData callout;

        public string Id => id;
        public GameState Trigger => trigger;
        public TutorialScene Scene => scene;
        public bool AfterFirstBattle => afterFirstBattle;
        public TutorialStepData Callout => callout;

        public static TutorialTipData Create(string id, GameState trigger, TutorialScene scene, string targetName,
            bool afterFirstBattle = false)
        {
            return new TutorialTipData
            {
                id = id,
                trigger = trigger,
                scene = scene,
                afterFirstBattle = afterFirstBattle,
                callout = TutorialStepData.Create(id, TutorialStepKind.Info, scene, targetName: targetName)
            };
        }
    }
}
