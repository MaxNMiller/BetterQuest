using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Events;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class AttackPanelView : MonoBehaviour
    {
        private const int SlotCount = 5;

        [SerializeField] private HabitSlotEventChannelSO onHabitSlotSelected;
        [Tooltip("Optional: reduce-motion / duration scale for the completion feedback.")]
        [SerializeField] private UiMotionSettingsSO motionSettings;

        private UIDocument document;
        private readonly Button[] buttons = new Button[SlotCount];
        private readonly VisualElement[] badges = new VisualElement[SlotCount];
        private readonly Label[] labels = new Label[SlotCount];
        private readonly Label[] _elementLabels = new Label[SlotCount];
        private readonly Label[] _levelLabels = new Label[SlotCount];
        private readonly Label[] _countLabels = new Label[SlotCount];
        private readonly VisualElement[] _flashes = new VisualElement[SlotCount];
        private readonly Spaa.Habits.HabitInstance[] _shownHabits = new Spaa.Habits.HabitInstance[SlotCount];
        private readonly bool[] _built = new bool[SlotCount];
        private readonly System.Action[] clickHandlers = new System.Action[SlotCount];
        private VisualElement _layer;
        private VisualElement _monsterAnchor;

        private void OnEnable()
        {
            document = GetComponent<UIDocument>();
            var root = document.rootVisualElement;
            _layer = root.Q<VisualElement>("safe-area-root") ?? root;
            _monsterAnchor = root.Q<VisualElement>("monster-anchor");
            for (int i = 0; i < SlotCount; i++)
            {
                buttons[i] = root.Q<Button>($"slot-{i}");
                badges[i] = root.Q<VisualElement>($"slot-{i}-badge");
                labels[i] = root.Q<Label>($"slot-{i}-label");
                _elementLabels[i] = root.Q<Label>($"slot-{i}-element");
                _levelLabels[i] = root.Q<Label>($"slot-{i}-level");
                _countLabels[i] = root.Q<Label>($"slot-{i}-count");

                if (_flashes[i] == null || _flashes[i].parent != buttons[i])
                {
                    _flashes[i] = new VisualElement { pickingMode = PickingMode.Ignore };
                    _flashes[i].AddToClassList("slot-flash");
                    buttons[i].Add(_flashes[i]);
                }

                int index = i;
                clickHandlers[i] = () => HandleSlotClicked(index);
                buttons[i].clicked += clickHandlers[i];
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                buttons[i].clicked -= clickHandlers[i];
            }
        }

        public void BuildSlots(BattleState state, ElementDefinition[] elementDefinitions)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                _built[i] = false;
                RefreshSlot(i, state, elementDefinitions);
            }
        }

        public void RefreshSlot(int index, BattleState state, ElementDefinition[] elementDefinitions)
        {
            var slot = state.GetSlot(index);
            RefreshSlot(index, slot, elementDefinitions, state.GetElementLevel(slot.Element), state.RemainingFor(slot.Element));
        }

        public void RefreshSlot(int index, BattleSlot slot, ElementDefinition[] elementDefinitions)
        {
            RefreshSlot(index, slot, elementDefinitions, -1, 0);
        }

        public void RefreshSlot(int index, BattleSlot slot, ElementDefinition[] elementDefinitions, int level, int remaining)
        {
            var definition = elementDefinitions[(int)slot.Element];
            var button = buttons[index];
            ElementStyle.Apply(button, slot.Element);

            string elementName = definition != null && !string.IsNullOrEmpty(definition.DisplayName)
                ? definition.DisplayName
                : slot.Element.ToString();
            _elementLabels[index].text = elementName.ToUpperInvariant();
            _levelLabels[index].text = $"Lv {level}";
            _levelLabels[index].style.display = level >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _countLabels[index].text = $"+{remaining}";
            _countLabels[index].style.display = remaining > 0 && !slot.IsDisabled ? DisplayStyle.Flex : DisplayStyle.None;

            bool changed = _built[index] && _shownHabits[index] != slot.CurrentHabit;
            string newText = slot.IsDisabled ? "All done!" : slot.CurrentHabit.Definition.DisplayName;

            if (changed)
            {
                PlayCompletion(index, slot.Element, newText);
            }
            else
            {
                labels[index].text = newText;
            }

            button.SetEnabled(!slot.IsDisabled);
            button.EnableInClassList("attack-slot--disabled", slot.IsDisabled);
            _shownHabits[index] = slot.CurrentHabit;
            _built[index] = true;
        }

        private void PlayCompletion(int index, Element element, string newText)
        {
            if (UiMotionSettingsSO.Reduced(motionSettings))
            {
                labels[index].text = newText;
                return;
            }

            float scale = UiMotionSettingsSO.Scale(motionSettings);
            UiMotion.Flash(_flashes[index], gameObject, 0.5f, 0.25f * scale);
            UiMotion.SwapText(labels[index], gameObject, newText, 0.25f * scale);
            if (_countLabels[index].resolvedStyle.display == DisplayStyle.Flex)
            {
                UiMotion.Punch(_countLabels[index], gameObject, 1.3f, 0.25f * scale);
            }

            SpawnOrb(index, element, 0.35f * scale);
        }

        private void SpawnOrb(int index, Element element, float duration)
        {
            if (_monsterAnchor == null || float.IsNaN(badges[index].worldBound.width))
            {
                return;
            }

            const float half = 28f;
            Vector2 from = _layer.WorldToLocal(badges[index].worldBound.center) - new Vector2(half, half);
            Vector2 to = _layer.WorldToLocal(_monsterAnchor.worldBound.center) - new Vector2(half, half);

            var orb = new VisualElement { pickingMode = PickingMode.Ignore };
            orb.AddToClassList("completion-orb");
            orb.AddToClassList("el-icon");
            ElementStyle.Apply(orb, element);
            _layer.Add(orb);
            UiMotion.FlyTo(orb, gameObject, from, to, duration);
        }

        private void HandleSlotClicked(int index)
        {
            if (onHabitSlotSelected != null)
            {
                onHabitSlotSelected.Raise(index);
            }
        }
    }
}
