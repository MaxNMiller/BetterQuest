using System;
using System.IO;
using NUnit.Framework;
using Spaa.Save;

namespace Spaa.Tests
{
    public class JsonSaveServiceTests
    {
        private string tempFilePath;

        [SetUp]
        public void SetUp()
        {
            tempFilePath = Path.Combine(Path.GetTempPath(), "spaa_save_test_" + Guid.NewGuid() + ".json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        [Test]
        public void Load_MissingFile_ReturnsDefaults()
        {
            var service = new JsonSaveService(tempFilePath);

            var data = service.Load();

            Assert.IsNotNull(data);
            Assert.AreEqual(100, data.playerHp);
            Assert.AreEqual(0, data.completedHabitIds.Count);
        }

        [Test]
        public void SaveThenLoad_RoundTrips()
        {
            var service = new JsonSaveService(tempFilePath);
            var data = new SaveData
            {
                playerHp = 42,
                currentDayKey = "2026-09-26",
                monsterSeed = 1234,
                lastMonsterSeed = 4321,
                monsterHp = 77,
                takeDamageEnabled = false
            };
            data.completedHabitIds.Add("habit_sleep");
            data.elementLevels[2] = 3;

            service.Save(data);
            var loaded = service.Load();

            Assert.AreEqual(42, loaded.playerHp);
            Assert.AreEqual("2026-09-26", loaded.currentDayKey);
            Assert.AreEqual(1234, loaded.monsterSeed);
            Assert.AreEqual(4321, loaded.lastMonsterSeed);
            Assert.AreEqual(77, loaded.monsterHp);
            Assert.IsFalse(loaded.takeDamageEnabled);
            Assert.AreEqual(1, loaded.completedHabitIds.Count);
            Assert.AreEqual("habit_sleep", loaded.completedHabitIds[0]);
            Assert.AreEqual(3, loaded.elementLevels[2]);
        }

        [Test]
        public void Load_CorruptFile_ReturnsDefaultsAndDoesNotThrow()
        {
            File.WriteAllText(tempFilePath, "{ not valid json !!! ][");
            var service = new JsonSaveService(tempFilePath);

            SaveData data = null;
            Assert.DoesNotThrow(() => data = service.Load());

            Assert.IsNotNull(data);
            Assert.AreEqual(100, data.playerHp);
        }
    }
}
