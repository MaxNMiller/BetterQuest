using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Spaa.Elements;
using Spaa.Progression;
using Spaa.Save;

namespace Spaa.Tests
{
    public class HistoryRecorderTests
    {
        private static readonly int[] Expected = { 2, 2, 4, 2, 2 };

        [Test]
        public void BeginDay_SameSeedTwice_DoesNotDuplicate()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);

            var first = recorder.BeginDay("2026-09-27", 111, Element.Food, 84, Expected);
            var second = recorder.BeginDay("2026-09-27", 111, Element.Food, 84, Expected);

            Assert.AreEqual(1, history.Count);
            Assert.AreSame(first, second);
            Assert.AreEqual((int)Element.Food, first.weakness);
            Assert.AreEqual(4, first.expectedPerElement[(int)Element.Food]);
        }

        [Test]
        public void BeginDay_NewSeedSameDate_OpensNewRecord()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);

            recorder.BeginDay("2026-09-27", 111, Element.Food, 84, Expected);
            recorder.BeginDay("2026-09-27", 222, Element.Move, 84, Expected);

            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(222, history[1].monsterSeed);
        }

        [Test]
        public void RecordHit_AccumulatesPerElementWeaknessAndBiggest()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);
            recorder.BeginDay("2026-09-27", 111, Element.Food, 84, Expected);

            recorder.RecordHit(111, Element.Food, 20, true);
            recorder.RecordHit(111, Element.Rest, 10, false);
            recorder.RecordHit(111, Element.Food, 25, true);

            var day = history[0];
            Assert.AreEqual(2, day.completedPerElement[(int)Element.Food]);
            Assert.AreEqual(45, day.damagePerElement[(int)Element.Food]);
            Assert.AreEqual(1, day.completedPerElement[(int)Element.Rest]);
            Assert.AreEqual(3, day.totalHits);
            Assert.AreEqual(2, day.weaknessHits);
            Assert.AreEqual(25, day.biggestHit);
            Assert.AreEqual(55, day.TotalDamage);
            Assert.AreEqual(3, day.TotalCompleted);
        }

        [Test]
        public void RecordHit_UnknownSeed_IsIgnored()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);
            recorder.BeginDay("2026-09-27", 111, Element.Food, 84, Expected);

            Assert.DoesNotThrow(() => recorder.RecordHit(999, Element.Food, 20, true));
            Assert.AreEqual(0, history[0].totalHits);
        }

        [Test]
        public void RecordDefeatAndEndOfDay_SetFields()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);
            recorder.BeginDay("2026-09-26", 111, Element.Rest, 84, Expected);
            recorder.BeginDay("2026-09-27", 222, Element.Move, 84, Expected);

            recorder.RecordEndOfDayAttack(111, 8);
            recorder.RecordDefeat(222);

            Assert.IsFalse(history[0].defeated);
            Assert.AreEqual(8, history[0].endOfDayDamageTaken);
            Assert.IsTrue(history[1].defeated);
        }

        [Test]
        public void BeginDay_OverCap_DropsOldest()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);

            for (int i = 1; i <= HistoryRecorder.MaxRecords + 5; i++)
            {
                recorder.BeginDay("2026-01-01", i, Element.Rest, 10, Expected);
            }

            Assert.AreEqual(HistoryRecorder.MaxRecords, history.Count);
            Assert.AreEqual(6, history[0].monsterSeed);
        }

        [Test]
        public void RecordHit_RecordWithMissingArrays_IsRepaired()
        {
            var history = new List<DayRecord> { new DayRecord { dayKey = "2026-09-27", monsterSeed = 5, completedPerElement = null, damagePerElement = new int[2] } };
            var recorder = new HistoryRecorder(history);

            recorder.RecordHit(5, Element.Move, 12, false);

            Assert.AreEqual(1, history[0].completedPerElement[(int)Element.Move]);
            Assert.AreEqual(12, history[0].damagePerElement[(int)Element.Move]);
        }

        [Test]
        public void History_RoundTripsThroughJsonSaveService()
        {
            string path = Path.Combine(Path.GetTempPath(), "spaa_history_test_" + Guid.NewGuid() + ".json");
            try
            {
                var data = new SaveData();
                var recorder = new HistoryRecorder(data.history);
                recorder.BeginDay("2026-09-27", 111, Element.Food, 84, Expected);
                recorder.RecordHit(111, Element.Food, 20, true);
                recorder.RecordDefeat(111);

                var service = new JsonSaveService(path);
                service.Save(data);
                var loaded = service.Load();

                Assert.AreEqual(1, loaded.history.Count);
                var day = loaded.history[0];
                Assert.AreEqual("2026-09-27", day.dayKey);
                Assert.AreEqual(111, day.monsterSeed);
                Assert.IsTrue(day.defeated);
                Assert.AreEqual(20, day.damagePerElement[(int)Element.Food]);
                Assert.AreEqual(4, day.expectedPerElement[(int)Element.Food]);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Test]
        public void LegacySaveWithoutHistory_LoadsEmptyList()
        {
            var loaded = JsonUtility.FromJson<SaveData>("{\"playerHp\":50,\"currentDayKey\":\"2026-09-20\"}");

            Assert.IsNotNull(loaded.history);
            Assert.AreEqual(0, loaded.history.Count);
        }
    }
}
