using UnityEngine;
using UnityEngine.UIElements;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SafeAreaUtility : MonoBehaviour
    {
        private const float ShortHeight = 1700f;
        private const string ShortClass = "is-short";
        private const string BleedClass = "safe-bleed-bottom";

        [SerializeField] private string safeAreaRootName = "safe-area-root";

        private UIDocument document;
        private VisualElement safeAreaRoot;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake()
        {
            document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            var root = document.rootVisualElement;
            safeAreaRoot = root != null ? root.Q(safeAreaRootName) : null;
            _lastScreen = Vector2Int.zero;
            if (safeAreaRoot != null)
            {
                safeAreaRoot.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            }
        }

        private void OnDisable()
        {
            if (safeAreaRoot != null)
            {
                safeAreaRoot.UnregisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            }
        }

        private void Update()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            var safeArea = Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == _lastSafeArea && screen == _lastScreen)
            {
                return;
            }

            _lastSafeArea = safeArea;
            _lastScreen = screen;
            var margins = SafeAreaMath.ComputeMargins(safeArea, screen.x, screen.y);
            safeAreaRoot.style.marginLeft = margins.Left;
            safeAreaRoot.style.marginRight = margins.Right;
            safeAreaRoot.style.marginTop = margins.Top;
            safeAreaRoot.style.marginBottom = margins.Bottom;
            ApplyBottomBleed(margins.Bottom);
        }

        private void ApplyBottomBleed(float inset)
        {
            safeAreaRoot.Query(className: BleedClass).ForEach(element =>
            {
                if (!(element.userData is float basePadding))
                {
                    basePadding = element.resolvedStyle.paddingBottom;
                    element.userData = basePadding;
                }

                element.style.marginBottom = -inset;
                element.style.paddingBottom = basePadding + inset;
            });
        }

        private void HandleGeometryChanged(GeometryChangedEvent evt)
        {
            safeAreaRoot.EnableInClassList(ShortClass, evt.newRect.height < ShortHeight);
        }
    }
}
