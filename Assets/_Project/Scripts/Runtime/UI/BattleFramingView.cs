using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class BattleFramingView : MonoBehaviour
    {
        [Tooltip("Composers of the idle / hit / victory cameras; all aim at the monster.")]
        [SerializeField] private CinemachineRotationComposer[] composers;
        [SerializeField] private string topElementName = "nameplate";
        [SerializeField] private string bottomElementName = "attack-sheet";
        [SerializeField] private string anchorElementName = "monster-anchor";
        [Tooltip("Largest vertical composer shift (0.5 = half the screen).")]
        [SerializeField] private float maxOffset = 0.3f;

        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _top;
        private VisualElement _bottom;
        private VisualElement _anchor;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            _root = _document.rootVisualElement;
            if (_root == null)
            {
                return;
            }

            _top = _root.Q<VisualElement>(topElementName);
            _bottom = _root.Q<VisualElement>(bottomElementName);
            _anchor = _root.Q<VisualElement>(anchorElementName);
            Register(_top);
            Register(_bottom);
        }

        private void OnDisable()
        {
            Unregister(_top);
            Unregister(_bottom);
        }

        private void Register(VisualElement element)
        {
            if (element != null)
            {
                element.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            }
        }

        private void Unregister(VisualElement element)
        {
            if (element != null)
            {
                element.UnregisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            }
        }

        private void HandleGeometryChanged(GeometryChangedEvent evt)
        {
            Reframe();
        }

        private void Reframe()
        {
            if (_top == null || _bottom == null)
            {
                return;
            }

            float height = _root.worldBound.height;
            if (float.IsNaN(height) || height <= 0f)
            {
                return;
            }

            float topCovered = _top.worldBound.yMax / height;
            float bottomCovered = (height - _bottom.worldBound.yMin) / height;
            float composerY = ScreenFramingMath.ComposerY(topCovered, bottomCovered, maxOffset);

            if (composers != null)
            {
                foreach (var composer in composers)
                {
                    if (composer == null)
                    {
                        continue;
                    }

                    var composition = composer.Composition;
                    composition.ScreenPosition.y = composerY;
                    composer.Composition = composition;
                }
            }

            if (_anchor != null && _anchor.parent != null)
            {
                float centreWorldY = (0.5f + composerY) * height;
                _anchor.style.top = _anchor.parent.WorldToLocal(new Vector2(0f, centreWorldY)).y;
            }
        }
    }
}
