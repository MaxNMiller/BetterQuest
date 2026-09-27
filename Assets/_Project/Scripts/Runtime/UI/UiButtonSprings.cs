using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Events;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UiButtonSprings : MonoBehaviour
    {
        [Tooltip("Optional: raised whenever an enabled Button in this document is clicked.")]
        [SerializeField] private VoidEventChannelSO onButtonClicked;

        private VisualElement _root;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            if (_root != null)
            {
                _root.RegisterCallback<PointerDownEvent>(HandlePointerDown, TrickleDown.TrickleDown);
                _root.RegisterCallback<ClickEvent>(HandleClick, TrickleDown.TrickleDown);
            }
        }

        private void OnDisable()
        {
            if (_root != null)
            {
                _root.UnregisterCallback<PointerDownEvent>(HandlePointerDown, TrickleDown.TrickleDown);
                _root.UnregisterCallback<ClickEvent>(HandleClick, TrickleDown.TrickleDown);
            }
        }

        private void HandlePointerDown(PointerDownEvent evt)
        {
            var button = FindButton(evt.target as VisualElement);
            if (button != null)
            {
                UiMotion.PressSpring(button, gameObject);
            }
        }

        private void HandleClick(ClickEvent evt)
        {
            if (onButtonClicked != null && FindButton(evt.target as VisualElement) != null)
            {
                onButtonClicked.Raise();
            }
        }

        private static Button FindButton(VisualElement element)
        {
            while (element != null && !(element is Button))
            {
                element = element.parent;
            }

            return element != null && element.enabledInHierarchy ? (Button)element : null;
        }
    }
}
