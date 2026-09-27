using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Spaa.Elements;
using Spaa.Events;
using Spaa.Flow;
using Spaa.Habits;
using Spaa.Save;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class HabitMenuView : MonoBehaviour
    {
        private const float TapSlop = 24f;
        private const int CounterWarnAt = 20;

        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [SerializeField] private HabitDefinition[] habitPool;
        [SerializeField] private VisualTreeAsset cardTemplate;
        [Tooltip("Indexed by Element enum value; supplies display names and taglines.")]
        [SerializeField] private ElementDefinition[] elementDefinitions;
        [Tooltip("Optional: reduce-motion skips the validation shake.")]
        [SerializeField] private UiMotionSettingsSO motionSettings;
        [Tooltip("Optional (tutorial): raised after the habit editor opens.")]
        [SerializeField] private VoidEventChannelSO onHabitEditorOpened;
        [Tooltip("Optional (tutorial): raised whenever the habit editor closes, including after a save.")]
        [SerializeField] private VoidEventChannelSO onHabitEditorClosed;
        [Tooltip("Optional (tutorial): raised after a habit is added or edited and saved, before the editor closes.")]
        [SerializeField] private VoidEventChannelSO onHabitSaved;
        [Tooltip("Optional: raised when the delete confirmation pops up (popup SFX).")]
        [SerializeField] private VoidEventChannelSO onPopupShown;

        private VisualElement panel;
        private VisualElement listContainer;
        private Button backButton;
        private Button addButton;
        private Label missingWarning;

        private VisualElement editOverlay;
        private VisualElement editCard;
        private Label editTitle;
        private TextField nameField;
        private VisualElement elementPicker;
        private Label timesValueLabel;
        private Button timesMinusButton;
        private Button timesPlusButton;
        private Label errorLabel;
        private Button saveButton;
        private Button cancelButton;
        private Button deleteButton;

        private ScrollView _scroll;
        private Label _summary;
        private Label _counter;
        private Label _tagline;
        private VisualElement _timesPips;
        private Button _closeButton;
        private VisualElement _editButtons;
        private VisualElement _deleteConfirm;
        private VisualElement _deleteDivider;
        private Label _deleteQuestion;
        private Label _deleteWarning;
        private Button _deleteKeepButton;
        private Button _deleteConfirmButton;

        private readonly List<Button> _elementChips = new List<Button>();
        private ISaveService _saveService;
        private SaveData _saveData;
        private CustomHabitCatalog _catalog;

        private string _editingId;
        private Element _draftElement;
        private int _draftTimesPerDay;
        private Vector2 _pointerDownPosition;

        private void OnEnable()
        {
            _saveService = new JsonSaveService(SaveLocation.DefaultFilePath);

            var root = GetComponent<UIDocument>().rootVisualElement;
            panel = root.Q<VisualElement>("habit-menu-panel");
            listContainer = root.Q<VisualElement>("habit-list");
            backButton = root.Q<Button>("habit-menu-back-button");
            addButton = root.Q<Button>("habit-add-button");
            missingWarning = root.Q<Label>("habit-missing-warning");

            editOverlay = root.Q<VisualElement>("habit-edit-overlay");
            editCard = root.Q<VisualElement>("habit-edit-card");
            editTitle = root.Q<Label>("habit-edit-title");
            nameField = root.Q<TextField>("habit-name-field");
            elementPicker = root.Q<VisualElement>("habit-element-picker");
            timesValueLabel = root.Q<Label>("habit-times-value");
            timesMinusButton = root.Q<Button>("habit-times-minus");
            timesPlusButton = root.Q<Button>("habit-times-plus");
            errorLabel = root.Q<Label>("habit-edit-error");
            saveButton = root.Q<Button>("habit-save-button");
            cancelButton = root.Q<Button>("habit-cancel-button");
            deleteButton = root.Q<Button>("habit-delete-button");

            _scroll = root.Q<ScrollView>("habit-list-scroll");
            _summary = root.Q<Label>("habit-summary");
            _counter = root.Q<Label>("habit-name-counter");
            _tagline = root.Q<Label>("habit-element-tagline");
            _timesPips = root.Q<VisualElement>("habit-times-pips");
            _closeButton = root.Q<Button>("habit-close-button");
            _editButtons = root.Q<VisualElement>("habit-edit-buttons");
            _deleteConfirm = root.Q<VisualElement>("habit-delete-confirm");
            _deleteDivider = root.Q<VisualElement>("habit-delete-divider");
            _deleteQuestion = root.Q<Label>("habit-delete-question");
            _deleteWarning = root.Q<Label>("habit-delete-warning");
            _deleteKeepButton = root.Q<Button>("habit-delete-keep");
            _deleteConfirmButton = root.Q<Button>("habit-delete-confirm-button");

            nameField.maxLength = CustomHabitCatalog.MaxNameLength;
            nameField.textEdition.placeholder = "e.g. Drink a glass of water";
            BuildElementPicker();
            BuildTimesPips();

            backButton.clicked += HandleBackClicked;
            addButton.clicked += HandleAddClicked;
            timesMinusButton.clicked += HandleTimesMinus;
            timesPlusButton.clicked += HandleTimesPlus;
            saveButton.clicked += HandleSaveClicked;
            cancelButton.clicked += CloseEditor;
            _closeButton.clicked += CloseEditor;
            deleteButton.clicked += HandleDeleteClicked;
            _deleteKeepButton.clicked += HideDeleteConfirm;
            _deleteConfirmButton.clicked += HandleDeleteConfirmed;
            nameField.RegisterValueChangedCallback(HandleNameChanged);
            editCard.RegisterCallback<KeyDownEvent>(HandleEditorKeyDown, TrickleDown.TrickleDown);

            if (onRequestGameState != null)
            {
                onRequestGameState.RegisterListener(HandleStateRequested);
            }
        }

        private void OnDisable()
        {
            backButton.clicked -= HandleBackClicked;
            addButton.clicked -= HandleAddClicked;
            timesMinusButton.clicked -= HandleTimesMinus;
            timesPlusButton.clicked -= HandleTimesPlus;
            saveButton.clicked -= HandleSaveClicked;
            cancelButton.clicked -= CloseEditor;
            _closeButton.clicked -= CloseEditor;
            deleteButton.clicked -= HandleDeleteClicked;
            _deleteKeepButton.clicked -= HideDeleteConfirm;
            _deleteConfirmButton.clicked -= HandleDeleteConfirmed;
            nameField.UnregisterValueChangedCallback(HandleNameChanged);
            editCard.UnregisterCallback<KeyDownEvent>(HandleEditorKeyDown, TrickleDown.TrickleDown);

            if (onRequestGameState != null)
            {
                onRequestGameState.UnregisterListener(HandleStateRequested);
            }
        }

        private void HandleStateRequested(GameState state)
        {
            bool show = state == GameState.HabitMenu;
            panel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            editOverlay.style.display = DisplayStyle.None;

            if (!show)
            {
                return;
            }

            _saveData = _saveService.Load();
            if (HabitRoster.EnsureSeeded(_saveData, habitPool))
            {
                _saveService.Save(_saveData);
            }

            _catalog = new CustomHabitCatalog(_saveData);
            RebuildList();
            _scroll.scrollOffset = Vector2.zero;

            for (int i = 0; i < listContainer.childCount; i++)
            {
                UiMotion.SlideUpIn(listContainer[i], gameObject, 80f, Mathf.Min(0.04f * i, 0.5f));
            }
        }

        private void RebuildList()
        {
            listContainer.Clear();
            RefreshMissingWarning();

            var groups = HabitJournalGrouping.Group(_saveData.customHabits);
            int total = 0;
            foreach (var group in groups)
            {
                total += group.Habits.Count;
            }

            _summary.text = total == 0 ? "No habits yet"
                : total == 1 ? "1 habit · tap it to edit"
                : $"{total} habits · tap one to edit";

            if (total == 0)
            {
                _scroll.scrollOffset = Vector2.zero;
                listContainer.Add(CreateEmptyJournal());
                return;
            }

            foreach (var group in groups)
            {
                listContainer.Add(CreateSectionHeader(group.Element, group.Habits.Count));
                if (group.Habits.Count == 0)
                {
                    listContainer.Add(CreateEmptyRow(group.Element));
                    continue;
                }

                foreach (var custom in group.Habits)
                {
                    listContainer.Add(CreateCard(custom.displayName, group.Element, custom.timesPerDay, custom.id));
                }
            }
        }

        private VisualElement CreateSectionHeader(Element element, int count)
        {
            var header = new VisualElement();
            header.AddToClassList("section-header");
            ElementStyle.Apply(header, element);
            header.Add(CreateBadge("element-badge--xs"));

            var name = new Label(GetElementName(element).ToUpperInvariant());
            name.AddToClassList("section-header__name");
            header.Add(name);

            var chip = new Label(count.ToString());
            chip.AddToClassList("chip");
            chip.AddToClassList("chip--count");
            header.Add(chip);
            return header;
        }

        private VisualElement CreateCard(string displayName, Element element, int timesPerDay, string habitId)
        {
            VisualElement card = cardTemplate != null ? cardTemplate.Instantiate() : new VisualElement();
            card.AddToClassList("card-root");
            var body = card.Q<VisualElement>("card-body") ?? card;
            ElementStyle.Apply(body, element);
            body.Add(CreateBadge("element-badge--sm"));

            var text = new VisualElement();
            text.AddToClassList("habit-row__text");

            var nameLabel = new Label(displayName);
            nameLabel.AddToClassList("t-body-lg");
            nameLabel.AddToClassList("t-clamp-2");
            nameLabel.AddToClassList("habit-row__name");
            text.Add(nameLabel);

            int times = CustomHabitCatalog.ClampTimesPerDay(timesPerDay);
            var meta = new VisualElement();
            meta.AddToClassList("habit-row__meta");
            meta.Add(CreatePips(times));
            var timesLabel = new Label($"{times}× daily");
            timesLabel.AddToClassList("t-caption");
            timesLabel.AddToClassList("habit-row__times");
            meta.Add(timesLabel);
            text.Add(meta);
            body.Add(text);

            var edit = new VisualElement();
            edit.AddToClassList("icon");
            edit.AddToClassList("icon--sm");
            edit.AddToClassList("icon--edit");
            edit.AddToClassList("habit-row__edit");
            body.Add(edit);

            RegisterTap(body, () => OpenEditor(habitId));
            return card;
        }

        private VisualElement CreateEmptyRow(Element element)
        {
            var row = new VisualElement();
            row.AddToClassList("row--empty");
            ElementStyle.Apply(row, element);

            var plus = new VisualElement();
            plus.AddToClassList("icon");
            plus.AddToClassList("icon--sm");
            plus.AddToClassList("icon--plus");
            row.Add(plus);

            var label = new Label($"Add a {GetElementName(element)} habit");
            label.AddToClassList("row--empty__label");
            row.Add(label);

            RegisterTap(row, () => OpenEditor(null, element));
            return row;
        }

        private VisualElement CreateEmptyJournal()
        {
            var empty = new VisualElement();
            empty.AddToClassList("journal-empty");

            var icon = new VisualElement();
            icon.AddToClassList("icon");
            icon.AddToClassList("icon--xl");
            icon.AddToClassList("icon--book");
            empty.Add(icon);

            var title = new Label("Your journal is empty");
            title.AddToClassList("t-h2");
            title.AddToClassList("journal-empty__title");
            empty.Add(title);

            var body = new Label("Add a habit for each element to start battling.");
            body.AddToClassList("t-body");
            body.AddToClassList("journal-empty__body");
            empty.Add(body);

            var add = new Button(HandleAddClicked);
            add.AddToClassList("btn");
            add.AddToClassList("btn--primary");
            var addIcon = new VisualElement();
            addIcon.AddToClassList("icon");
            addIcon.AddToClassList("icon--sm");
            addIcon.AddToClassList("icon--plus");
            add.Add(addIcon);
            var addLabel = new Label("New Habit");
            addLabel.AddToClassList("btn__label");
            add.Add(addLabel);
            empty.Add(add);
            return empty;
        }

        private static VisualElement CreateBadge(string sizeClass)
        {
            var badge = new VisualElement();
            badge.AddToClassList("element-badge");
            badge.AddToClassList(sizeClass);
            var icon = new VisualElement();
            icon.AddToClassList("element-badge__icon");
            icon.AddToClassList("el-icon");
            badge.Add(icon);
            return badge;
        }

        private static VisualElement CreatePips(int filled)
        {
            var pips = new VisualElement();
            pips.AddToClassList("pips");
            for (int i = 0; i < CustomHabitCatalog.MaxTimesPerDay; i++)
            {
                var pip = new VisualElement();
                pip.AddToClassList("pip");
                pip.EnableInClassList("pip--on", i < filled);
                pips.Add(pip);
            }

            return pips;
        }

        private void RegisterTap(VisualElement target, Action onTap)
        {
            target.RegisterCallback<PointerDownEvent>(evt =>
            {
                _pointerDownPosition = evt.position;
                UiMotion.PressSpring(target, gameObject);
            }, TrickleDown.TrickleDown);
            target.RegisterCallback<ClickEvent>(evt =>
            {
                if (((Vector2)evt.position - _pointerDownPosition).sqrMagnitude <= TapSlop * TapSlop)
                {
                    onTap();
                }
            });
        }

        private void RefreshMissingWarning()
        {
            var missing = HabitRoster.MissingElements(_saveData.customHabits);
            missingWarning.style.display = missing.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            if (missing.Count > 0)
            {
                missingWarning.text = $"Add a {JoinElementNames(missing)} habit to battle.";
            }
        }

        private string JoinElementNames(List<Element> elements)
        {
            var names = new string[elements.Count];
            for (int i = 0; i < elements.Count; i++)
            {
                names[i] = GetElementName(elements[i]);
            }

            if (names.Length <= 1)
            {
                return names.Length == 1 ? names[0] : string.Empty;
            }

            return string.Join(", ", names, 0, names.Length - 1) + " and " + names[names.Length - 1];
        }

        private void BuildElementPicker()
        {
            elementPicker.Clear();
            _elementChips.Clear();

            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                var chip = new Button();
                chip.AddToClassList("habit-element-chip");
                chip.AddToClassList("element-tile");
                ElementStyle.Apply(chip, element);

                var icon = new VisualElement();
                icon.AddToClassList("el-icon");
                chip.Add(icon);

                var name = new Label(GetElementName(element));
                name.AddToClassList("element-tile__name");
                chip.Add(name);

                Element captured = element;
                chip.clicked += () => SelectElement(captured, true);
                elementPicker.Add(chip);
                _elementChips.Add(chip);
            }
        }

        private void BuildTimesPips()
        {
            _timesPips.Clear();
            for (int i = 0; i < CustomHabitCatalog.MaxTimesPerDay; i++)
            {
                var pip = new VisualElement();
                pip.AddToClassList("pip");
                _timesPips.Add(pip);
            }
        }

        private void OpenEditor(string customId)
        {
            OpenEditor(customId, null);
        }

        private void OpenEditor(string customId, Element? preset)
        {
            var existing = customId != null ? _catalog.Find(customId) : null;
            _editingId = existing != null ? existing.id : null;

            editTitle.text = existing != null ? "Edit Habit" : "New Habit";
            nameField.SetValueWithoutNotify(existing != null ? existing.displayName : string.Empty);
            _draftTimesPerDay = existing != null ? CustomHabitCatalog.ClampTimesPerDay(existing.timesPerDay) : CustomHabitCatalog.MinTimesPerDay;
            SelectElement(existing != null ? (Element)existing.element : preset.GetValueOrDefault(Element.Rest), false);
            RefreshTimes(false);
            RefreshCounter(nameField.value);
            ClearError();
            HideDeleteConfirm();

            bool editing = existing != null;
            deleteButton.style.display = editing ? DisplayStyle.Flex : DisplayStyle.None;
            _deleteDivider.style.display = editing ? DisplayStyle.Flex : DisplayStyle.None;
            editOverlay.style.display = DisplayStyle.Flex;

            UiMotion.FadeIn(editOverlay, gameObject);
            UiMotion.PopIn(editCard, gameObject);

            if (!editing)
            {
                nameField.schedule.Execute(() => nameField.Focus()).StartingIn(50);
            }

            if (onHabitEditorOpened != null)
            {
                onHabitEditorOpened.Raise();
            }
        }

        private void CloseEditor()
        {
            _editingId = null;
            nameField.Blur();
            UiMotion.PopOut(editCard, gameObject, () => editOverlay.style.display = DisplayStyle.None);

            if (onHabitEditorClosed != null)
            {
                onHabitEditorClosed.Raise();
            }
        }

        private void SelectElement(Element element, bool animate)
        {
            _draftElement = element;
            for (int i = 0; i < _elementChips.Count; i++)
            {
                bool selected = i == (int)element;
                bool wasSelected = _elementChips[i].ClassListContains("habit-element-chip--selected");
                _elementChips[i].EnableInClassList("habit-element-chip--selected", selected);
                _elementChips[i].EnableInClassList("element-tile--selected", selected);
                if (animate && selected && !wasSelected)
                {
                    UiMotion.Punch(_elementChips[i], gameObject, 1.06f, 0.25f);
                }
            }

            ElementStyle.Apply(_timesPips, element);
            ElementStyle.Apply(_tagline, element);
            var definition = GetDefinition(element);
            _tagline.text = definition != null && !string.IsNullOrEmpty(definition.Tagline)
                ? $"{GetElementName(element)}: {definition.Tagline}"
                : GetElementName(element);
        }

        private void RefreshTimes(bool animate)
        {
            timesValueLabel.text = _draftTimesPerDay.ToString();
            timesMinusButton.SetEnabled(_draftTimesPerDay > CustomHabitCatalog.MinTimesPerDay);
            timesPlusButton.SetEnabled(_draftTimesPerDay < CustomHabitCatalog.MaxTimesPerDay);
            for (int i = 0; i < _timesPips.childCount; i++)
            {
                _timesPips[i].EnableInClassList("pip--on", i < _draftTimesPerDay);
            }

            if (animate)
            {
                UiMotion.Punch(timesValueLabel, gameObject);
            }
        }

        private void HandleTimesMinus()
        {
            _draftTimesPerDay = CustomHabitCatalog.ClampTimesPerDay(_draftTimesPerDay - 1);
            RefreshTimes(true);
        }

        private void HandleTimesPlus()
        {
            _draftTimesPerDay = CustomHabitCatalog.ClampTimesPerDay(_draftTimesPerDay + 1);
            RefreshTimes(true);
        }

        private void HandleNameChanged(ChangeEvent<string> evt)
        {
            RefreshCounter(evt.newValue);
            ClearError();
        }

        private void RefreshCounter(string value)
        {
            int length = value != null ? value.Length : 0;
            _counter.text = $"{length}/{CustomHabitCatalog.MaxNameLength}";
            _counter.EnableInClassList("field__counter--warn", length >= CounterWarnAt);
        }

        private void ClearError()
        {
            errorLabel.style.visibility = Visibility.Hidden;
            nameField.RemoveFromClassList("field--invalid");
        }

        private void HandleEditorKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                CloseEditor();
                evt.StopPropagation();
            }
            else if ((evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                     && _deleteConfirm.resolvedStyle.display == DisplayStyle.None)
            {
                HandleSaveClicked();
                evt.StopPropagation();
            }
        }

        private void HandleSaveClicked()
        {
            bool ok = _editingId != null
                ? _catalog.TryUpdate(_editingId, nameField.value, _draftElement, _draftTimesPerDay)
                : _catalog.TryAdd(nameField.value, _draftElement, _draftTimesPerDay, out _);

            if (!ok)
            {
                errorLabel.style.visibility = Visibility.Visible;
                nameField.AddToClassList("field--invalid");
                if (!UiMotionSettingsSO.Reduced(motionSettings))
                {
                    UiMotion.Shake(editCard, gameObject, 12f, 0.3f, 4);
                }

                return;
            }

            CommitAndClose(true);
        }

        private void HandleDeleteClicked()
        {
            if (_editingId == null)
            {
                return;
            }

            var existing = _catalog.Find(_editingId);
            string name = existing != null ? existing.displayName : "this habit";
            _deleteQuestion.text = $"Delete “{name}”?";

            int sameElement = 0;
            foreach (var habit in _saveData.customHabits)
            {
                if (habit != null && existing != null && habit.element == existing.element)
                {
                    sameElement++;
                }
            }

            bool onlyOne = existing != null && sameElement <= 1;
            _deleteWarning.style.display = onlyOne ? DisplayStyle.Flex : DisplayStyle.None;
            if (onlyOne)
            {
                string elementName = GetElementName((Element)existing.element);
                _deleteWarning.text = $"This is your only {elementName} habit, so you'll need a new one before battling.";
            }

            _editButtons.style.display = DisplayStyle.None;
            deleteButton.style.display = DisplayStyle.None;
            _deleteDivider.style.display = DisplayStyle.None;
            _deleteConfirm.style.display = DisplayStyle.Flex;
            UiMotion.PopIn(_deleteConfirm, gameObject);
            if (onPopupShown != null)
            {
                onPopupShown.Raise();
            }
        }

        private void HideDeleteConfirm()
        {
            _deleteConfirm.style.display = DisplayStyle.None;
            _editButtons.style.display = DisplayStyle.Flex;
            bool editing = _editingId != null;
            deleteButton.style.display = editing ? DisplayStyle.Flex : DisplayStyle.None;
            _deleteDivider.style.display = editing ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void HandleDeleteConfirmed()
        {
            if (_editingId != null)
            {
                _catalog.Remove(_editingId);
            }

            CommitAndClose(false);
        }

        private void CommitAndClose(bool habitSaved)
        {
            _saveService.Save(_saveData);

            if (habitSaved && onHabitSaved != null)
            {
                onHabitSaved.Raise();
            }

            CloseEditor();
            RebuildList();
        }

        private void HandleAddClicked()
        {
            OpenEditor(null);
        }

        private void HandleBackClicked()
        {
            if (onRequestGameState != null)
            {
                onRequestGameState.Raise(GameState.MainMenu);
            }
        }

        private ElementDefinition GetDefinition(Element element)
        {
            int index = (int)element;
            if (elementDefinitions == null || index >= elementDefinitions.Length)
            {
                return null;
            }

            return elementDefinitions[index];
        }

        private string GetElementName(Element element)
        {
            var definition = GetDefinition(element);
            return definition != null && !string.IsNullOrEmpty(definition.DisplayName) ? definition.DisplayName : element.ToString();
        }
    }
}
