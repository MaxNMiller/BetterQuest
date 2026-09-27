using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spaa.DayCycle;
using Spaa.DebugTools;
using Spaa.Elements;
using Spaa.Events;
using Spaa.Flow;
using Spaa.Habits;
using Spaa.Progression;
using Spaa.Save;
using Spaa.UI;

namespace Spaa.Battle
{
    public class BattleController : MonoBehaviour
    {
        [SerializeField] private int playerMaxHp = 100;
        [SerializeField] private bool takeDamageEnabled = true;
        [SerializeField] private float presentationDelay = 0.5f;
        [Tooltip("Seconds the monster's end-of-day attack plays before the Soft Reminder opens (spec 2.6).")]
        [SerializeField] private float endOfDayAttackDuration = 1f;

        [SerializeField] private HabitDefinition[] habitPool;
        [SerializeField] private ElementDefinition[] elementDefinitions;

        [SerializeField] private ArenaBuilder arenaBuilder;
        [SerializeField] private MonsterView monsterView;
        [SerializeField] private HudView hudView;
        [SerializeField] private AttackPanelView attackPanelView;

        [SerializeField] private HabitSlotEventChannelSO onHabitSlotSelected;
        [SerializeField] private AttackResultEventChannelSO onAttackResolved;
        [SerializeField] private VoidEventChannelSO onMonsterDefeated;
        [SerializeField] private IntEventChannelSO onPlayerDamaged;
        [SerializeField] private GameStateEventChannelSO onRequestGameState;

        private BattleState state;
        private BattleCommandInvoker invoker;
        private ElementLevels elementLevels;
        private bool inputLocked;

        private ISaveService saveService;
        private SaveData saveData;
        private DebugDayClock dayClock;
        private DayCycleService dayCycleService;
        private HistoryRecorder history;
        private List<HabitDefinition> activePool;

        public ElementLevels ElementLevels => elementLevels;
        public MonsterData Monster => state?.Monster;
        public bool TakeDamageEnabled => state != null && state.TakeDamageEnabled;
        public bool IsMonsterDefeated => state != null && state.IsMonsterDefeated;
        public DebugDayClock DayClock => dayClock;

        private void Start()
        {
            saveService = new JsonSaveService(SaveLocation.DefaultFilePath);
            saveData = saveService.Load();
            HabitRoster.EnsureSeeded(saveData, habitPool);
            activePool = HabitPoolBuilder.Build(null, saveData.customHabits);

            dayClock = new DebugDayClock(new SystemDayClock().GetCurrentDayKey());
            dayCycleService = new DayCycleService(dayClock);

            elementLevels = new LevelUpService().LoadFrom(saveData);

            string today = dayClock.GetCurrentDayKey();
            bool resumeSameDay = saveData.currentDayKey == today && saveData.monsterSeed != 0;
            int previousSeed = saveData.monsterSeed != 0 ? saveData.monsterSeed : saveData.lastMonsterSeed;
            int seed = resumeSameDay ? saveData.monsterSeed : DailyMonsterGenerator.NewSeed(previousSeed);
            if (!resumeSameDay)
            {
                saveData.lastMonsterSeed = previousSeed;
            }

            double totalExpectedNeutralDamage = 0;
            foreach (var habit in activePool)
            {
                totalExpectedNeutralDamage += habit.BasePower * Mathf.Max(1, habit.TimesPerDay);
            }

            var monster = DailyMonsterGenerator.Generate(seed, totalExpectedNeutralDamage);
            var queue = new HabitQueue(activePool, seed);
            bool resumedTakeDamage = resumeSameDay ? saveData.takeDamageEnabled : takeDamageEnabled;
            state = new BattleState(monster, queue, elementLevels, playerMaxHp, resumedTakeDamage);
            invoker = new BattleCommandInvoker(state);

            if (saveData.history == null)
            {
                saveData.history = new List<DayRecord>();
            }

            history = new HistoryRecorder(saveData.history);
            history.BeginDay(today, seed, monster.Weakness, monster.MaxHp, ExpectedPerElement(activePool));

            if (resumeSameDay)
            {
                ResumeCompletedHabits();
                state.SetMonsterHp(saveData.monsterHp);
                state.SetPlayerHp(saveData.playerHp);
            }
            else
            {
                saveData.completedHabitIds.Clear();
                saveData.levelUpPending = false;
            }

            var weaknessDefinition = elementDefinitions[(int)monster.Weakness];
            arenaBuilder.Build(weaknessDefinition);
            monsterView.Setup(weaknessDefinition);
            monsterView.SetHealth(state.MonsterHp, state.Monster.MaxHp);
            hudView.Bind(monster, elementLevels.GetLevel(monster.Weakness), weaknessDefinition);
            hudView.SetHp(state.MonsterHp, state.Monster.MaxHp);
            attackPanelView.BuildSlots(state, elementDefinitions);

            SaveProgress();

            if (state.IsMonsterDefeated)
            {
                HandleAlreadyDefeatedOnLoad();
            }
        }

        private void HandleAlreadyDefeatedOnLoad()
        {
            inputLocked = true;
            if (onRequestGameState == null)
            {
                return;
            }

            onRequestGameState.Raise(saveData.levelUpPending ? GameState.LevelUp : GameState.MainMenu);
        }

        private void ResumeCompletedHabits()
        {
            var completedCountByElement = new Dictionary<Element, int>();
            var habitById = new Dictionary<string, HabitDefinition>();
            foreach (var habit in activePool)
            {
                if (!habitById.ContainsKey(habit.Id))
                {
                    habitById[habit.Id] = habit;
                }
            }

            foreach (var id in saveData.completedHabitIds)
            {
                if (habitById.TryGetValue(id, out var habit))
                {
                    completedCountByElement.TryGetValue(habit.Element, out int count);
                    completedCountByElement[habit.Element] = count + 1;
                }
            }

            for (int i = 0; i < state.Slots.Count; i++)
            {
                var slot = state.GetSlot(i);
                if (!completedCountByElement.TryGetValue(slot.Element, out int completedCount))
                {
                    continue;
                }

                HabitInstance next = slot.CurrentHabit;
                for (int drawn = 0; drawn < completedCount; drawn++)
                {
                    next = state.DrawNext(slot.Element);
                }

                slot.Refill(next);
            }
        }

        private static int[] ExpectedPerElement(List<HabitDefinition> pool)
        {
            var expected = new int[DayRecord.ElementCount];
            foreach (var habit in pool)
            {
                expected[(int)habit.Element] += Mathf.Max(1, habit.TimesPerDay);
            }

            return expected;
        }

        private void SaveProgress()
        {
            saveData.monsterHp = state.MonsterHp;
            saveData.playerHp = state.PlayerHp;
            saveData.takeDamageEnabled = state.TakeDamageEnabled;
            saveData.monsterSeed = state.Monster.Seed;
            saveData.currentDayKey = dayClock.GetCurrentDayKey();
            new LevelUpService().SaveTo(saveData, elementLevels);
            saveService.Save(saveData);
        }

        private void OnEnable()
        {
            if (onHabitSlotSelected != null)
            {
                onHabitSlotSelected.RegisterListener(HandleSlotSelected);
            }
        }

        private void OnDisable()
        {
            if (onHabitSlotSelected != null)
            {
                onHabitSlotSelected.UnregisterListener(HandleSlotSelected);
            }
        }

        private void HandleSlotSelected(int slotIndex)
        {
            if (inputLocked || state == null)
            {
                return;
            }

            var slotBeforeAttack = state.GetSlot(slotIndex);
            string completedHabitId = slotBeforeAttack.IsDisabled ? null : slotBeforeAttack.CurrentHabit.Definition.Id;

            var result = invoker.ExecuteCommand(new HabitAttackCommand(slotIndex));
            if (!result.Success)
            {
                return;
            }

            if (completedHabitId != null)
            {
                saveData.completedHabitIds.Add(completedHabitId);
            }

            history.RecordHit(state.Monster.Seed, result.Element, result.DamageDealt, result.WasWeakness);
            if (result.MonsterDefeated)
            {
                saveData.levelUpPending = true;
                history.RecordDefeat(state.Monster.Seed);
            }

            inputLocked = true;
            monsterView.SetHealth(state.MonsterHp, state.Monster.MaxHp);
            onAttackResolved?.Raise(result);
            hudView.SetHp(state.MonsterHp, state.Monster.MaxHp);
            attackPanelView.RefreshSlot(slotIndex, state, elementDefinitions);
            SaveProgress();

            StartCoroutine(UnlockAfterDelay(result));
        }

        private IEnumerator UnlockAfterDelay(CommandResult result)
        {
            yield return new WaitForSeconds(presentationDelay);

            if (result.MonsterDefeated)
            {
                onMonsterDefeated?.Raise();
                onRequestGameState?.Raise(GameState.LevelUp);
            }
            else
            {
                inputLocked = false;
            }
        }

        public void ApplyLevelUp(Element element)
        {
            new LevelUpService().ApplyUpgrade(elementLevels, element);
            saveData.levelUpPending = false;
            SaveProgress();
        }

        public void ForceEndOfDay()
        {
            if (state == null || state.IsMonsterDefeated)
            {
                Debug.Log("BattleController: ForceEndOfDay skipped, monster already defeated.");
                return;
            }

            int damage = EndOfDayDamageCalculator.Calculate(state.Monster.MaxHp);
            var result = invoker.ExecuteCommand(new MonsterEndOfDayAttackCommand(damage));
            onPlayerDamaged?.Raise(result.DamageDealt);
            history.RecordEndOfDayAttack(state.Monster.Seed, result.DamageDealt);
            SaveProgress();
            StartCoroutine(ShowReminderAfterAttack());
        }

        private IEnumerator ShowReminderAfterAttack()
        {
            inputLocked = true;
            yield return new WaitForSeconds(endOfDayAttackDuration);
            onRequestGameState?.Raise(GameState.SoftReminder);
        }

        public void AdvanceDay()
        {
            dayClock.AdvanceDay();
            var outcome = dayCycleService.CheckRollover(saveData.currentDayKey, state.IsMonsterDefeated);

            if (outcome == DayRolloverOutcome.NewDayMonsterSurvived)
            {
                ForceEndOfDay();
            }

            DebugDayActions.StartFreshDay(saveData);
            saveService.Save(saveData);
            Debug.Log($"BattleController: AdvanceDay -> {outcome}. Reopen Battle to see the next day's monster.");
        }

        public void KillMonster()
        {
            if (state == null)
            {
                return;
            }

            state.SetMonsterHp(0);
            hudView.SetHp(0, state.Monster.MaxHp);
            saveData.levelUpPending = true;
            history.RecordDefeat(state.Monster.Seed);
            SaveProgress();
            onMonsterDefeated?.Raise();
            onRequestGameState?.Raise(GameState.LevelUp);
        }

        public void ResetSave()
        {
            saveService.Save(new SaveData());
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void SetTakeDamage(bool enabled)
        {
            if (state == null)
            {
                return;
            }

            state.TakeDamageEnabled = enabled;
            SaveProgress();
        }

        public void SeedDemoHistory()
        {
            DemoHistoryGenerator.ReplaceHistory(saveData.history, dayClock.GetCurrentDayKey(), DemoHistoryGenerator.DefaultSeed);
            SaveProgress();
        }

        public void ClearHistory()
        {
            history.Clear();
            history.BeginDay(dayClock.GetCurrentDayKey(), state.Monster.Seed, state.Monster.Weakness, state.Monster.MaxHp,
                ExpectedPerElement(activePool));
            SaveProgress();
        }

        public void DebugLevelUp(Element element)
        {
            elementLevels.IncrementLevel(element);
            SaveProgress();
        }
    }
}
