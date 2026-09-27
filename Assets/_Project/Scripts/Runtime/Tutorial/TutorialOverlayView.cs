using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Elements;
using Spaa.Events;
using Spaa.UI;

namespace Spaa.Tutorial
{
    [RequireComponent(typeof(UIDocument))]
    public class TutorialOverlayView : MonoBehaviour
    {
        private const float TargetTimeout = 1.5f;
        private const long TrackIntervalMs = 33;
        private const float ArrowSize = 44f;
        private const float ArrowCornerInset = 56f;
        private const float FallbackDialogueHeight = 420f;
        private const string CompactClass = "tut-dialogue--compact";
        private const string PortraitClass = "tut-avatar__body--portrait";

        [SerializeField] private TutorialConfigSO config;
        [Tooltip("Documents searched for step targets by name: MenuUI in the Menu; HUD and Overlays in Battle.")]
        [SerializeField] private UIDocument[] targetDocuments = new UIDocument[0];
        [Tooltip("Optional: raised when a dialogue or the replay prompt pops up (popup SFX).")]
        [SerializeField] private VoidEventChannelSO onPopupShown;

        public event Action NextClicked;
        public event Action NotYetClicked;
        public event Action SkipClicked;
        public event Action LearnMoreClicked;
        public event Action ReplayConfirmed;
        public event Action ReplayDeclined;
        public event Action FallbackEntered;

        private readonly VisualElement[] _dims = new VisualElement[SpotlightMath.RectCount];
        private readonly Rect[] _dimRects = new Rect[SpotlightMath.RectCount];

        private VisualElement _root;
        private VisualElement _tutRoot;
        private VisualElement _blocker;
        private VisualElement _ring;
        private VisualElement _safe;
        private VisualElement _dialogue;
        private VisualElement _arrowUp;
        private VisualElement _arrowDown;
        private VisualElement _avatarBody;
        private VisualElement _avatarEye;
        private VisualElement _avatarMouth;
        private Label _name;
        private Label _promptName;
        private VisualElement _lines;
        private VisualElement _elements;
        private Label _footnote;
        private Button _skip;
        private Button _learnMore;
        private Button _notYet;
        private Label _notYetLabel;
        private Button _next;
        private Label _nextLabel;
        private VisualElement _prompt;
        private Label _promptText;
        private Button _promptYes;
        private Button _promptNo;

        private IVisualElementScheduledItem _tracker;
        private TutorialStepData _step;
        private string _targetName;
        private string _holeTargetName;
        private bool _dim = true;
        private bool _fallback;
        private VisualElement _target;
        private VisualElement _holeTarget;
        private float _shownAt;
        private Rect _lastHole;
        private Rect _lastSafe;
        private float _lastDialogueHeight;
        private Vector2 _lastPanelSize;
        private float _bobY;
        private bool _compact;

        public bool IsShowingStep => _step != null;
        public bool IsFallback => _fallback;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.pickingMode = PickingMode.Ignore;
            _tutRoot = _root.Q("tut-root");
            if (_tutRoot == null)
            {
                Debug.LogWarning("TutorialOverlayView: TutorialOverlay.uxml isn't assigned to this UIDocument.", this);
                return;
            }

            for (int i = 0; i < _dims.Length; i++)
            {
                _dims[i] = _root.Q($"tut-dim-{i}");
                _dims[i].RegisterCallback<PointerDownEvent>(HandleDimPointerDown, TrickleDown.TrickleDown);
            }

            _blocker = _root.Q("tut-blocker");
            _blocker.RegisterCallback<PointerDownEvent>(HandleDimPointerDown, TrickleDown.TrickleDown);
            _ring = _root.Q("tut-ring");
            _safe = _root.Q("safe-area-root");
            _dialogue = _root.Q("tut-dialogue");
            _arrowUp = _root.Q("tut-arrow-up");
            _arrowDown = _root.Q("tut-arrow-down");
            _avatarBody = _root.Q("tut-avatar-body");
            _avatarEye = _root.Q("tut-avatar-eye");
            _avatarMouth = _root.Q("tut-avatar-mouth");
            _name = _root.Q<Label>("tut-name");
            _promptName = _root.Q<Label>("tut-prompt-name");
            _lines = _root.Q("tut-lines");
            _elements = _root.Q("tut-elements");
            _footnote = _root.Q<Label>("tut-footnote");
            _skip = _root.Q<Button>("tut-skip");
            _learnMore = _root.Q<Button>("tut-learn-more");
            _notYet = _root.Q<Button>("tut-not-yet");
            _notYetLabel = _root.Q<Label>("tut-not-yet-label");
            _next = _root.Q<Button>("tut-next");
            _nextLabel = _root.Q<Label>("tut-next-label");
            _prompt = _root.Q("tut-prompt");
            _promptText = _root.Q<Label>("tut-prompt-text");
            _promptYes = _root.Q<Button>("tut-prompt-yes");
            _promptNo = _root.Q<Button>("tut-prompt-no");

            foreach (var button in new[] { _skip, _learnMore, _notYet, _next, _promptYes, _promptNo })
            {
                button.focusable = false;
            }

            _skip.clicked += RaiseSkip;
            _learnMore.clicked += RaiseLearnMore;
            _notYet.clicked += RaiseNotYet;
            _next.clicked += RaiseNext;
            _promptYes.clicked += RaiseReplayConfirmed;
            _promptNo.clicked += RaiseReplayDeclined;

            BuildElementIcons();
            _tracker = _root.schedule.Execute(Track).Every(TrackIntervalMs);
            _tracker.Pause();
            _step = null;
            SetVisible(false);
        }

        private void OnDisable()
        {
            if (_dialogue == null)
            {
                return;
            }

            _tracker?.Pause();
            for (int i = 0; i < _dims.Length; i++)
            {
                _dims[i].UnregisterCallback<PointerDownEvent>(HandleDimPointerDown, TrickleDown.TrickleDown);
            }

            _blocker.UnregisterCallback<PointerDownEvent>(HandleDimPointerDown, TrickleDown.TrickleDown);
            _skip.clicked -= RaiseSkip;
            _learnMore.clicked -= RaiseLearnMore;
            _notYet.clicked -= RaiseNotYet;
            _next.clicked -= RaiseNext;
            _promptYes.clicked -= RaiseReplayConfirmed;
            _promptNo.clicked -= RaiseReplayDeclined;
            KillMotion();
        }

        public void Show(TutorialStepData step, TutorialOverlayModel model, IReadOnlyList<string> lines,
            string targetOverride = null)
        {
            if (_dialogue == null || step == null)
            {
                return;
            }

            string targetName = string.IsNullOrEmpty(targetOverride) ? step.TargetName : targetOverride;
            bool sameStep = _step == step && _fallback == model.Fallback && _targetName == targetName;
            _step = step;
            _targetName = targetName;
            _holeTargetName = string.IsNullOrEmpty(targetOverride) ? step.HoleTargetName : string.Empty;
            _dim = model.Dim;
            _fallback = model.Fallback;
            if (!sameStep)
            {
                SetCompact(!_dim);
            }

            _target = null;
            _holeTarget = null;
            _shownAt = Time.unscaledTime;
            _lastHole = new Rect(float.NaN, 0f, 0f, 0f);

            _name.text = config != null ? config.GuideName : string.Empty;
            SetLines(lines ?? step.Lines);
            _footnote.text = step.Footnote ?? string.Empty;
            Display(_footnote, !string.IsNullOrEmpty(step.Footnote));
            Display(_elements, model.ShowElements);
            Display(_skip, model.ShowSkip);
            Display(_learnMore, model.ShowLearnMore);
            Display(_notYet, model.ShowNotYet);
            _notYetLabel.text = model.NotYetLabel;
            Display(_next, model.ShowNext);
            _nextLabel.text = model.NextLabel;
            _blocker.pickingMode = model.BlockHole ? PickingMode.Position : PickingMode.Ignore;
            ApplyFace(step);

            Display(_prompt, false);
            Display(_dialogue, true);
            SetVisible(true);
            Track();
            _tracker.Resume();

            if (!sameStep)
            {
                PlayEntrance();
                RaisePopupShown();
            }
        }

        public void ShowReplayPrompt(string text)
        {
            if (_dialogue == null)
            {
                return;
            }

            _step = null;
            _tracker.Pause();
            _promptName.text = config != null ? config.GuideName : string.Empty;
            _promptText.text = text;
            SpotlightMath.Compute(Rect.zero, PanelSize(), 0f, _dimRects);
            ApplyDims(false, Rect.zero);
            _blocker.pickingMode = PickingMode.Ignore;
            Display(_dialogue, false);
            Display(_prompt, true);
            SetVisible(true);
            if (!Reduced())
            {
                UiMotion.PopIn(_prompt, gameObject);
            }

            RaisePopupShown();
        }

        public void Hide()
        {
            if (_dialogue == null)
            {
                return;
            }

            _step = null;
            _fallback = false;
            _tracker.Pause();
            KillMotion();
            SetVisible(false);
        }

        private void Track()
        {
            if (_step == null || _root.panel == null)
            {
                return;
            }

            bool hasHole = false;
            Rect targetRect = Rect.zero;
            Rect holeRect = Rect.zero;
            if (!_fallback && !string.IsNullOrEmpty(_targetName))
            {
                if (_target == null || _target.panel == null)
                {
                    _target = Find(_targetName);
                }

                if (!string.IsNullOrEmpty(_holeTargetName) && (_holeTarget == null || _holeTarget.panel == null))
                {
                    _holeTarget = Find(_holeTargetName);
                }

                var holeElement = _holeTarget ?? _target;
                if (_target != null && IsShown(_target) && holeElement != null && IsShown(holeElement))
                {
                    targetRect = _target.worldBound;
                    holeRect = holeElement.worldBound;
                    hasHole = SpotlightMath.IsUsable(holeRect);
                }

                if (!hasHole)
                {
                    if (_dim && Time.unscaledTime - _shownAt > TargetTimeout)
                    {
                        EnterFallback();
                    }

                    if (SpotlightMath.IsUsable(_lastHole))
                    {
                        return;
                    }
                }
            }

            Vector2 panelSize = PanelSize();
            Rect safe = _safe.worldBound;
            if (!SpotlightMath.IsUsable(safe))
            {
                safe = new Rect(0f, 0f, panelSize.x, panelSize.y);
            }

            float dialogueHeight = _dialogue.resolvedStyle.height;
            if (float.IsNaN(dialogueHeight) || dialogueHeight <= 0f)
            {
                dialogueHeight = FallbackDialogueHeight;
            }

            if (holeRect == _lastHole && safe == _lastSafe && panelSize == _lastPanelSize
                && Mathf.Approximately(dialogueHeight, _lastDialogueHeight))
            {
                return;
            }

            _lastHole = holeRect;
            _lastSafe = safe;
            _lastPanelSize = panelSize;
            _lastDialogueHeight = dialogueHeight;

            float padding = config != null ? config.SpotlightPadding : 16f;
            float gap = config != null ? config.DialogueGap : 24f;
            hasHole = SpotlightMath.Compute(holeRect, panelSize, padding, _dimRects) && hasHole;
            Rect padded = hasHole ? SpotlightMath.PaddedHole(holeRect, panelSize, padding) : Rect.zero;
            if (!_dim)
            {
                for (int i = 0; i < _dimRects.Length; i++)
                {
                    _dimRects[i] = Rect.zero;
                }
            }

            ApplyDims(hasHole, padded);
            if (!_dim)
            {
                Place(_blocker, Rect.zero);
            }

            var placement = DialoguePlacement.Resolve(padded, hasHole, safe, dialogueHeight, gap, _step.Dock);
            if (placement.CoversHole && !_compact)
            {
                SetCompact(true);
                _lastDialogueHeight = -1f;
            }

            _dialogue.style.top = placement.Top - safe.y;

            float pointAtX = hasHole ? (SpotlightMath.IsUsable(targetRect) ? targetRect.center.x : padded.center.x) : 0f;
            Rect dialogueRect = _dialogue.worldBound;
            float arrowLeft = DialoguePlacement.ArrowX(pointAtX, dialogueRect.x, dialogueRect.width, ArrowSize, ArrowCornerInset);
            Display(_arrowUp, placement.Arrow == TutorialArrow.Up);
            Display(_arrowDown, placement.Arrow == TutorialArrow.Down);
            _arrowUp.style.left = arrowLeft;
            _arrowDown.style.left = arrowLeft;
        }

        private void ApplyDims(bool hasHole, Rect padded)
        {
            for (int i = 0; i < _dims.Length; i++)
            {
                Place(_dims[i], _dimRects[i]);
            }

            Place(_blocker, hasHole ? padded : Rect.zero);
            Display(_ring, hasHole);
            if (hasHole)
            {
                Place(_ring, padded);
            }
        }

        private void SetCompact(bool compact)
        {
            _compact = compact;
            _dialogue.EnableInClassList(CompactClass, compact);
        }

        private void EnterFallback()
        {
            if (_fallback)
            {
                return;
            }

            Debug.LogWarning($"Tutorial: target '{_targetName}' for step '{_step.Id}' isn't on screen; showing the fallback.", this);
            _fallback = true;
            FallbackEntered?.Invoke();
        }

        private VisualElement Find(string elementName)
        {
            foreach (var document in targetDocuments)
            {
                if (document == null || document.rootVisualElement == null)
                {
                    continue;
                }

                var found = document.rootVisualElement.Q(elementName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static bool IsShown(VisualElement element)
        {
            if (element.panel == null)
            {
                return false;
            }

            for (var current = element; current != null; current = current.parent)
            {
                if (current.resolvedStyle.display == DisplayStyle.None || current.resolvedStyle.visibility == Visibility.Hidden)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetLines(IReadOnlyList<string> lines)
        {
            _lines.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var label = new Label(lines[i]) { pickingMode = PickingMode.Ignore };
                label.AddToClassList("tut-line");
                if (i == 0)
                {
                    label.AddToClassList("tut-line--lead");
                }

                _lines.Add(label);
            }
        }

        private void BuildElementIcons()
        {
            _elements.Clear();
            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList("el-icon");
                icon.AddToClassList("tut-element");
                ElementStyle.Apply(icon, element);
                _elements.Add(icon);
            }
        }

        private void ApplyFace(TutorialStepData step)
        {
            ElementStyle.Apply(_avatarBody, step.FaceElement);
            var portrait = config != null ? config.Portrait : null;
            _avatarBody.EnableInClassList(PortraitClass, portrait != null);
            if (portrait != null)
            {
                _avatarBody.style.backgroundImage = new StyleBackground(portrait);
                _avatarEye.style.backgroundImage = StyleKeyword.None;
                _avatarMouth.style.backgroundImage = StyleKeyword.None;
                return;
            }

            _avatarBody.style.backgroundImage = StyleKeyword.Null;
            var library = config != null ? config.FaceLibrary : null;
            if (library == null)
            {
                _avatarEye.style.backgroundImage = StyleKeyword.None;
                _avatarMouth.style.backgroundImage = StyleKeyword.None;
                return;
            }

            var face = library.GetFace(step.Expression, step.FaceElement);
            SetFaceLayer(_avatarEye, face.Eye, library.FaceColor);
            SetFaceLayer(_avatarMouth, face.Mouth, library.FaceColor);
        }

        private static void SetFaceLayer(VisualElement layer, Texture2D texture, Color tint)
        {
            if (texture == null)
            {
                layer.style.backgroundImage = StyleKeyword.None;
                return;
            }

            layer.style.backgroundImage = new StyleBackground(texture);
            layer.style.unityBackgroundImageTintColor = tint;
        }

        private void PlayEntrance()
        {
            KillMotion();
            if (Reduced())
            {
                _dialogue.style.opacity = 1f;
                UiMotion.SetScale(_dialogue, 1f);
                _ring.style.opacity = 1f;
                return;
            }

            UiMotion.PopIn(_dialogue, gameObject);
            UiMotion.Pulse(_ring, gameObject, 0.45f, 1.4f);
            _bobY = 0f;
            DOTween.To(() => _bobY, SetBob, -8f, 1.4f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(_avatarBody)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private void SetBob(float value)
        {
            _bobY = value;
            _avatarBody.style.translate = new Translate(0f, value);
        }

        private void KillMotion()
        {
            DOTween.Kill(_dialogue, true);
            DOTween.Kill(_prompt, true);
            DOTween.Kill(_ring);
            DOTween.Kill(_avatarBody);
            _ring.style.opacity = 1f;
            _avatarBody.style.translate = new Translate(0f, 0f);
        }

        private bool Reduced()
        {
            return UiMotionSettingsSO.Reduced(config != null ? config.MotionSettings : null);
        }

        private Vector2 PanelSize()
        {
            return _root.panel != null ? _root.panel.visualTree.layout.size : Vector2.zero;
        }

        private void HandleDimPointerDown(PointerDownEvent evt)
        {
            var focused = _root.panel != null ? _root.panel.focusController.focusedElement as VisualElement : null;
            if (focused != null)
            {
                focused.schedule.Execute(() => focused.Focus());
            }
        }

        private void SetVisible(bool visible)
        {
            Display(_tutRoot, visible);
        }

        private static void Display(VisualElement element, bool visible)
        {
            var value = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (element.style.display != value)
            {
                element.style.display = value;
            }
        }

        private static void Place(VisualElement element, Rect rect)
        {
            element.style.left = rect.x;
            element.style.top = rect.y;
            element.style.width = rect.width;
            element.style.height = rect.height;
        }

        private void RaisePopupShown()
        {
            if (onPopupShown != null)
            {
                onPopupShown.Raise();
            }
        }

        private void RaiseNext() => NextClicked?.Invoke();
        private void RaiseNotYet() => NotYetClicked?.Invoke();
        private void RaiseSkip() => SkipClicked?.Invoke();
        private void RaiseLearnMore() => LearnMoreClicked?.Invoke();
        private void RaiseReplayConfirmed() => ReplayConfirmed?.Invoke();
        private void RaiseReplayDeclined() => ReplayDeclined?.Invoke();
    }
}
