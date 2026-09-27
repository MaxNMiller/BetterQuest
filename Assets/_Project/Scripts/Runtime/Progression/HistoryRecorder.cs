using System;
using System.Collections.Generic;
using Spaa.Elements;
using Spaa.Save;

namespace Spaa.Progression
{
    public class HistoryRecorder
    {
        public const int MaxRecords = 90;

        private readonly List<DayRecord> _history;

        public HistoryRecorder(List<DayRecord> history)
        {
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        public DayRecord BeginDay(string dayKey, int monsterSeed, Element weakness, int monsterMaxHp, int[] expectedPerElement)
        {
            var existing = Find(monsterSeed);
            if (existing != null)
            {
                return existing;
            }

            var record = new DayRecord
            {
                dayKey = dayKey ?? string.Empty,
                monsterSeed = monsterSeed,
                weakness = (int)weakness,
                monsterMaxHp = monsterMaxHp
            };

            if (expectedPerElement != null)
            {
                int count = Math.Min(expectedPerElement.Length, DayRecord.ElementCount);
                Array.Copy(expectedPerElement, record.expectedPerElement, count);
            }

            _history.Add(record);
            if (_history.Count > MaxRecords)
            {
                _history.RemoveRange(0, _history.Count - MaxRecords);
            }

            return record;
        }

        public void RecordHit(int monsterSeed, Element element, int damage, bool wasWeakness)
        {
            var record = Find(monsterSeed);
            if (record == null)
            {
                return;
            }

            Repair(record);
            int index = (int)element;
            record.completedPerElement[index]++;
            record.damagePerElement[index] += damage;
            record.totalHits++;
            if (wasWeakness)
            {
                record.weaknessHits++;
            }

            if (damage > record.biggestHit)
            {
                record.biggestHit = damage;
            }
        }

        public void RecordDefeat(int monsterSeed)
        {
            var record = Find(monsterSeed);
            if (record != null)
            {
                record.defeated = true;
            }
        }

        public void RecordEndOfDayAttack(int monsterSeed, int damageTaken)
        {
            var record = Find(monsterSeed);
            if (record != null)
            {
                record.endOfDayDamageTaken += damageTaken;
            }
        }

        public void Clear()
        {
            _history.Clear();
        }

        private DayRecord Find(int monsterSeed)
        {
            for (int i = _history.Count - 1; i >= 0; i--)
            {
                if (_history[i] != null && _history[i].monsterSeed == monsterSeed)
                {
                    return _history[i];
                }
            }

            return null;
        }

        private static void Repair(DayRecord record)
        {
            record.completedPerElement = Resize(record.completedPerElement);
            record.expectedPerElement = Resize(record.expectedPerElement);
            record.damagePerElement = Resize(record.damagePerElement);
        }

        private static int[] Resize(int[] values)
        {
            if (values != null && values.Length == DayRecord.ElementCount)
            {
                return values;
            }

            var resized = new int[DayRecord.ElementCount];
            if (values != null)
            {
                Array.Copy(values, resized, Math.Min(values.Length, DayRecord.ElementCount));
            }

            return resized;
        }
    }
}
