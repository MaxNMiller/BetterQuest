using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using Spaa.DayCycle;
using Spaa.Elements;
using Spaa.Events;
using Spaa.Flow;
using Spaa.Progression;
using Spaa.Save;

namespace Spaa.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DashboardView : MonoBehaviour
    {
        private const int ChartDays = 7;

        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [Tooltip("Indexed by Element enum value; display names for the element rows.")]
        [SerializeField] private ElementDefinition[] elementDefinitions;
        [SerializeField] private UiMotionSettingsSO motionSettings;
        [Tooltip("Battle scene: shows today's recap and a Continue button instead of Back.")]
        [SerializeField] private bool postBattle;

        private VisualElement _host;
        private ScrollView _scroll;
        private Label _subtitle;
        private VisualElement _recap;
        private Label _recapLine;
        private VisualElement _empty;
        private VisualElement[] _cards;
        private Label _streakCount;
        private Label _streakNote;
        private Label _streakBest;
        private VisualElement _dots;
        private Label _damageTotal;
        private Label _damageToday;
        private Label _damageAvg;
        private Label _damageBiggest;
        private Label _damageSuper;
        private Label _damageHits;
        private VisualElement _chart;
        private VisualElement _elementRows;
        private Label _winsValue;
        private Label _winsCaption;
        private Label _habitsValue;
        private Label _bestDayValue;
        private Label _bestDayCaption;
        private Label _hitsTakenValue;
        private Button _closeButton;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _host = root.Q<VisualElement>("dashboard-host");
            _scroll = root.Q<ScrollView>("dash-scroll");
            _subtitle = root.Q<Label>("dash-subtitle");
            _recap = root.Q<VisualElement>("dash-recap");
            _recapLine = root.Q<Label>("dash-recap-line");
            _empty = root.Q<VisualElement>("dash-empty");
            _cards = new[]
            {
                root.Q<VisualElement>("dash-streak-card"),
                root.Q<VisualElement>("dash-damage-card"),
                root.Q<VisualElement>("dash-element-card"),
                root.Q<VisualElement>("dash-insight-card")
            };
            _streakCount = root.Q<Label>("dash-streak-count");
            _streakNote = root.Q<Label>("dash-streak-note");
            _streakBest = root.Q<Label>("dash-streak-best");
            _dots = root.Q<VisualElement>("dash-dots");
            _damageTotal = root.Q<Label>("dash-damage-total");
            _damageToday = root.Q<Label>("dash-damage-today");
            _damageAvg = root.Q<Label>("dash-damage-avg");
            _damageBiggest = root.Q<Label>("dash-damage-biggest");
            _damageSuper = root.Q<Label>("dash-damage-super");
            _damageHits = root.Q<Label>("dash-damage-hits");
            _chart = root.Q<VisualElement>("dash-chart");
            _elementRows = root.Q<VisualElement>("dash-element-rows");
            _winsValue = root.Q<Label>("dash-insight-wins");
            _winsCaption = root.Q<Label>("dash-insight-wins-caption");
            _habitsValue = root.Q<Label>("dash-insight-habits");
            _bestDayValue = root.Q<Label>("dash-insight-best-day");
            _bestDayCaption = root.Q<Label>("dash-insight-best-day-caption");
            _hitsTakenValue = root.Q<Label>("dash-insight-hits-taken");
            _closeButton = root.Q<Button>("dash-close-button");

            if (postBattle)
            {
                root.Q<Label>("dash-close-label").text = "Continue";
                var closeIcon = root.Q<VisualElement>("dash-close-icon");
                closeIcon.RemoveFromClassList("icon--back");
                closeIcon.AddToClassList("icon--check");
            }

            _closeButton.clicked += HandleCloseClicked;
            _host.style.display = DisplayStyle.None;

            if (onRequestGameState != null)
            {
                onRequestGameState.RegisterListener(HandleStateRequested);
            }
        }

        private void OnDisable()
        {
            _closeButton.clicked -= HandleCloseClicked;

            if (onRequestGameState != null)
            {
                onRequestGameState.UnregisterListener(HandleStateRequested);
            }
        }

        private void HandleStateRequested(GameState state)
        {
            bool show = state == GameState.Dashboard;
            _host.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            if (show)
            {
                Refresh();
            }
        }

        private void HandleCloseClicked()
        {
            if (onRequestGameState != null)
            {
                onRequestGameState.Raise(GameState.MainMenu);
            }
        }

        private void Refresh()
        {
            var save = new JsonSaveService(SaveLocation.DefaultFilePath).Load();
            var history = save.history ?? new List<DayRecord>();
            string today = new SystemDayClock().GetCurrentDayKey();
            var stats = DashboardStatsCalculator.Compute(history, today);
            var levels = new LevelUpService().LoadFrom(save);
            var todayRecord = history.Count > 0 && history[history.Count - 1].dayKey == today ? history[history.Count - 1] : null;
            bool animate = !UiMotionSettingsSO.Reduced(motionSettings);

            _scroll.scrollOffset = Vector2.zero;
            _subtitle.text = stats.IsEmpty ? "Your journey starts today" : $"{stats.DaysPlayed} {Plural(stats.DaysPlayed, "day")} logged";
            ShowRecap(todayRecord);

            _empty.style.display = stats.IsEmpty ? DisplayStyle.Flex : DisplayStyle.None;
            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].style.display = stats.IsEmpty ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (stats.IsEmpty)
            {
                if (animate)
                {
                    UiMotion.PopIn(_empty, gameObject, 0.05f);
                }

                return;
            }

            FillStreak(stats);
            FillDamage(stats, animate);
            FillElements(stats, levels, animate);
            FillInsights(stats);

            if (animate)
            {
                for (int i = 0; i < _cards.Length; i++)
                {
                    UiMotion.SlideUpIn(_cards[i], gameObject, 120f, 0.05f + 0.08f * i);
                }
            }
        }

        private void ShowRecap(DayRecord todayRecord)
        {
            if (!postBattle || todayRecord == null)
            {
                _recap.style.display = DisplayStyle.None;
                return;
            }

            _recap.style.display = DisplayStyle.Flex;
            ElementStyle.Apply(_recap, ToElement(todayRecord.weakness));
            int habits = todayRecord.TotalCompleted;
            string outcome = todayRecord.defeated ? "Victory!" : "Still standing";
            _recapLine.text = $"{habits} {Plural(habits, "habit")} · {todayRecord.TotalDamage} damage · {outcome}";
        }

        private void FillStreak(DashboardStats stats)
        {
            _streakCount.text = stats.CurrentStreak.ToString(CultureInfo.InvariantCulture);
            _streakBest.text = $"Best {stats.BestStreak}";

            var today = stats.RecentDays[stats.RecentDays.Count - 1];
            bool doneToday = postBattle || today.Completed > 0 || today.Status == DashboardDayStatus.Defeated;
            if (doneToday)
            {
                _streakNote.text = "See you tomorrow!";
            }
            else if (stats.CurrentStreak == 0)
            {
                _streakNote.text = "Finish a habit to start one!";
            }
            else
            {
                _streakNote.text = "Finish a habit today to keep it!";
            }

            _dots.Clear();
            foreach (var day in stats.RecentDays)
            {
                var dot = new VisualElement();
                dot.AddToClassList("dash-dot");
                if (day.Weakness >= 0)
                {
                    ElementStyle.Apply(dot, ToElement(day.Weakness));
                }

                switch (day.Status)
                {
                    case DashboardDayStatus.Defeated:
                        dot.AddToClassList("dash-dot--defeated");
                        break;
                    case DashboardDayStatus.Partial:
                        dot.AddToClassList("dash-dot--partial");
                        break;
                    case DashboardDayStatus.Missed:
                        dot.AddToClassList("dash-dot--missed");
                        break;
                    default:
                        dot.AddToClassList("dash-dot--today");
                        if (day.Completed > 0)
                        {
                            dot.AddToClassList("dash-dot--partial");
                        }

                        break;
                }

                _dots.Add(dot);
            }
        }

        private void FillDamage(DashboardStats stats, bool animate)
        {
            _damageTotal.text = stats.TotalDamage.ToString(CultureInfo.InvariantCulture);
            _damageToday.text = stats.TodayDamage.ToString(CultureInfo.InvariantCulture);
            _damageAvg.text = Mathf.RoundToInt(stats.AverageDamagePerDay).ToString(CultureInfo.InvariantCulture);
            _damageBiggest.text = stats.BiggestHit.ToString(CultureInfo.InvariantCulture);
            _damageSuper.text = Percent(stats.SuperEffectiveRatio);
            _damageHits.text = stats.TotalHits.ToString(CultureInfo.InvariantCulture);

            _chart.Clear();
            var days = stats.RecentDays;
            int start = Math.Max(0, days.Count - ChartDays);
            int max = 1;
            for (int i = start; i < days.Count; i++)
            {
                max = Math.Max(max, days[i].Damage);
            }

            for (int i = start; i < days.Count; i++)
            {
                var column = CreateBar(days[i], max, i == days.Count - 1);
                _chart.Add(column);
                if (animate)
                {
                    UiMotion.SlideUpIn(column, gameObject, 60f, 0.25f + 0.05f * (i - start));
                }
            }
        }

        private VisualElement CreateBar(DashboardDay day, int max, bool isToday)
        {
            var column = new VisualElement();
            column.AddToClassList("dash-bar");
            column.EnableInClassList("dash-bar--missed", day.Status == DashboardDayStatus.Missed);
            column.EnableInClassList("dash-bar--today", isToday);
            if (day.Weakness >= 0)
            {
                ElementStyle.Apply(column, ToElement(day.Weakness));
            }

            var head = new VisualElement();
            head.AddToClassList("dash-bar__head");
            var value = new Label(day.Damage > 0 ? day.Damage.ToString(CultureInfo.InvariantCulture) : string.Empty);
            value.AddToClassList("t-num");
            value.AddToClassList("dash-bar__value");
            head.Add(value);
            if (day.Status == DashboardDayStatus.Defeated)
            {
                var win = new VisualElement();
                win.AddToClassList("icon");
                win.AddToClassList("icon--check");
                win.AddToClassList("dash-bar__win");
                head.Add(win);
            }

            var track = new VisualElement();
            track.AddToClassList("dash-bar__track");
            var fill = new VisualElement();
            fill.AddToClassList("dash-bar__fill");
            fill.style.height = Length.Percent(100f * day.Damage / max);
            track.Add(fill);

            var label = new Label(Weekday(day.DayKey, isToday));
            label.AddToClassList("dash-bar__day");

            column.Add(head);
            column.Add(track);
            column.Add(label);
            return column;
        }

        private void FillElements(DashboardStats stats, ElementLevels levels, bool animate)
        {
            _elementRows.Clear();
            for (int e = 0; e < DayRecord.ElementCount; e++)
            {
                var element = (Element)e;
                var row = new VisualElement();
                row.AddToClassList("dash-element-row");
                ElementStyle.Apply(row, element);

                var badge = new VisualElement();
                badge.AddToClassList("element-badge");
                badge.AddToClassList("element-badge--sm");
                var icon = new VisualElement();
                icon.AddToClassList("element-badge__icon");
                icon.AddToClassList("el-icon");
                badge.Add(icon);

                var body = new VisualElement();
                body.AddToClassList("dash-element-row__body");

                var top = new VisualElement();
                top.AddToClassList("dash-element-row__top");
                var name = new Label(ElementName(element));
                name.AddToClassList("dash-element-row__name");
                var level = new Label($"Lv {levels.GetLevel(element)}");
                level.AddToClassList("t-caption");
                level.AddToClassList("t-num");
                level.AddToClassList("dash-element-row__level");
                top.Add(name);
                top.Add(level);

                if (stats.FavouriteElement == element)
                {
                    top.Add(CreateTag("FAVOURITE", false));
                }
                else if (stats.NeglectedElement == element)
                {
                    top.Add(CreateTag("NEEDS LOVE", true));
                }

                var count = new Label(stats.CompletedPerElement[e].ToString(CultureInfo.InvariantCulture));
                count.AddToClassList("t-num");
                count.AddToClassList("dash-element-row__count");
                top.Add(count);

                var bar = new VisualElement();
                bar.AddToClassList("bar");
                bar.AddToClassList("dash-element-row__bar");
                var fill = new VisualElement();
                fill.AddToClassList("bar__fill");
                bar.Add(fill);
                float share = stats.ShareOfTotal[e];
                if (animate)
                {
                    UiMotion.BarTo(fill, gameObject, 0f, share, 0.6f, 0.3f + 0.06f * e, Ease.OutCubic);
                }
                else
                {
                    UiMotion.SetScaleX(fill, share);
                }

                var detail = new Label($"{Percent(share)} of all habits · {Percent(stats.CompletionRate[e])} of planned done");
                detail.AddToClassList("t-caption");
                detail.AddToClassList("dash-element-row__detail");

                body.Add(top);
                body.Add(bar);
                body.Add(detail);
                row.Add(badge);
                row.Add(body);
                _elementRows.Add(row);
            }
        }

        private static VisualElement CreateTag(string text, bool warm)
        {
            var tag = new VisualElement();
            tag.AddToClassList("chip");
            tag.AddToClassList("dash-element-row__tag");
            tag.EnableInClassList("chip--weak", warm);
            var label = new Label(text);
            label.AddToClassList("chip__label");
            tag.Add(label);
            return tag;
        }

        private void FillInsights(DashboardStats stats)
        {
            _winsValue.text = $"{stats.MonstersDefeated} / {stats.DaysPlayed}";
            _winsCaption.text = $"Monsters beaten ({Percent(stats.WinRate)})";
            _habitsValue.text = stats.TotalCompleted.ToString(CultureInfo.InvariantCulture);
            _bestDayValue.text = stats.BestDayDamage > 0 ? stats.BestDayDamage.ToString(CultureInfo.InvariantCulture) : "-";
            _bestDayCaption.text = stats.BestDayDamage > 0 ? $"Best day · {ShortDate(stats.BestDayKey)}" : "Best day";
            _hitsTakenValue.text = stats.EndOfDayHitsTaken.ToString(CultureInfo.InvariantCulture);
        }

        private string ElementName(Element element)
        {
            int index = (int)element;
            if (elementDefinitions != null && index < elementDefinitions.Length && elementDefinitions[index] != null
                && !string.IsNullOrEmpty(elementDefinitions[index].DisplayName))
            {
                return elementDefinitions[index].DisplayName;
            }

            return element.ToString();
        }

        private static Element ToElement(int value)
        {
            return (Element)Mathf.Clamp(value, 0, DayRecord.ElementCount - 1);
        }

        private static string Percent(float ratio)
        {
            return Mathf.RoundToInt(ratio * 100f).ToString(CultureInfo.InvariantCulture) + "%";
        }

        private static string Plural(int count, string word)
        {
            return count == 1 ? word : word + "s";
        }

        private static string Weekday(string dayKey, bool isToday)
        {
            if (isToday)
            {
                return "Today";
            }

            return TryParseDay(dayKey, out var date) ? date.ToString("ddd", CultureInfo.InvariantCulture) : string.Empty;
        }

        private static string ShortDate(string dayKey)
        {
            return TryParseDay(dayKey, out var date) ? date.ToString("MMM d", CultureInfo.InvariantCulture) : dayKey;
        }

        private static bool TryParseDay(string dayKey, out DateTime date)
        {
            return DateTime.TryParseExact(dayKey, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
    }
}
