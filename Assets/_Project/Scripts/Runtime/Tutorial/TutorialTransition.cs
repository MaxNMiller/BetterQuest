using System;
using UnityEngine;
using Spaa.Flow;

namespace Spaa.Tutorial
{
    [Serializable]
    public class TutorialTransition
    {
        [SerializeField] private TutorialSignal on;
        [Tooltip("Only used when On is StateEntered.")]
        [SerializeField] private GameState state;
        [Tooltip("Step to go to. Empty completes the tutorial.")]
        [SerializeField] private string toStepId;
        [Tooltip("Only honoured on a replay (e.g. 'Skip for now' on the required first-habit steps).")]
        [SerializeField] private bool replayOnly;

        public TutorialSignal On => on;
        public GameState State => state;
        public string ToStepId => toStepId;
        public bool ReplayOnly => replayOnly;

        public TutorialTransition(TutorialSignal on, string toStepId, GameState state = default, bool replayOnly = false)
        {
            this.on = on;
            this.toStepId = toStepId;
            this.state = state;
            this.replayOnly = replayOnly;
        }

        public bool Matches(TutorialEvent evt)
        {
            return on == evt.Signal && (on != TutorialSignal.StateEntered || state == evt.State);
        }
    }
}
