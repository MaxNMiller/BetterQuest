using System;
using UnityEngine;
using Spaa.Battle;
using Spaa.Elements;

namespace Spaa.Tutorial
{
    [Serializable]
    public class TutorialStepData
    {
        [SerializeField] private string id;
        [SerializeField] private TutorialStepKind kind;
        [SerializeField] private TutorialScene scene;
        [Tooltip("UXML name of the element to call out. Empty = no spotlight.")]
        [SerializeField] private string targetName;
        [Tooltip("Optional wider element for the tappable hole (e.g. the whole editor card).")]
        [SerializeField] private string holeTargetName;
        [SerializeField] private TutorialDock dock;
        [TextArea(2, 4)]
        [SerializeField] private string[] lines = new string[0];
        [Tooltip("Small print under the lines (e.g. the medical disclaimer).")]
        [SerializeField] private string footnote;
        [SerializeField] private TutorialTransition[] transitions = new TutorialTransition[0];
        [Tooltip("Step to show instead when this scene is reloaded on this step (e.g. the habit editor is no longer open). Empty = this step.")]
        [SerializeField] private string resumeStepId;
        [Tooltip("On a first run, hide Skip on this step until the player has written a habit.")]
        [SerializeField] private bool lockSkipUntilFirstHabit;
        [Tooltip("Label of the secondary button that raises NotYet, when the step has a usable NotYet transition.")]
        [SerializeField] private string notYetLabel;
        [Tooltip("Wombino's face: expression layered over this element's face.")]
        [SerializeField] private MonsterExpression expression;
        [SerializeField] private Element faceElement;
        [Tooltip("Shows the Learn more button (opens the config's learnMoreUrl).")]
        [SerializeField] private bool showLearnMore;
        [Tooltip("Shows the five element icons under the lines.")]
        [SerializeField] private bool showElements;

        public string Id => id;
        public TutorialStepKind Kind => kind;
        public TutorialScene Scene => scene;
        public string TargetName => targetName;
        public string HoleTargetName => holeTargetName;
        public TutorialDock Dock => dock;
        public string[] Lines => lines;
        public string Footnote => footnote;
        public TutorialTransition[] Transitions => transitions;
        public string ResumeStepId => resumeStepId;
        public bool LockSkipUntilFirstHabit => lockSkipUntilFirstHabit;
        public string NotYetLabel => notYetLabel;
        public MonsterExpression Expression => expression;
        public Element FaceElement => faceElement;
        public bool ShowLearnMore => showLearnMore;
        public bool ShowElements => showElements;

        public static TutorialStepData Create(string id, TutorialStepKind kind, TutorialScene scene,
            TutorialTransition[] transitions = null, string resumeStepId = null, bool lockSkipUntilFirstHabit = false,
            string targetName = null)
        {
            return new TutorialStepData
            {
                id = id,
                kind = kind,
                scene = scene,
                transitions = transitions ?? new TutorialTransition[0],
                resumeStepId = resumeStepId ?? string.Empty,
                lockSkipUntilFirstHabit = lockSkipUntilFirstHabit,
                targetName = targetName ?? string.Empty,
                holeTargetName = string.Empty,
                footnote = string.Empty,
                notYetLabel = string.Empty,
                faceElement = Element.Rest
            };
        }

        public TutorialStepData WithLines(params string[] value)
        {
            lines = value ?? new string[0];
            return this;
        }

        public TutorialStepData WithHole(string holeTarget, TutorialDock dockSide = TutorialDock.Auto)
        {
            holeTargetName = holeTarget ?? string.Empty;
            dock = dockSide;
            return this;
        }

        public TutorialStepData WithResume(string stepId)
        {
            resumeStepId = stepId ?? string.Empty;
            return this;
        }

        public TutorialStepData WithDock(TutorialDock dockSide)
        {
            dock = dockSide;
            return this;
        }

        public TutorialStepData WithFootnote(string value)
        {
            footnote = value ?? string.Empty;
            return this;
        }

        public TutorialStepData WithNotYetLabel(string value)
        {
            notYetLabel = value ?? string.Empty;
            return this;
        }

        public TutorialStepData WithFace(Element element, MonsterExpression faceExpression = MonsterExpression.Neutral)
        {
            faceElement = element;
            expression = faceExpression;
            return this;
        }

        public TutorialStepData WithLearnMore()
        {
            showLearnMore = true;
            return this;
        }

        public TutorialStepData WithElements()
        {
            showElements = true;
            return this;
        }
    }
}
