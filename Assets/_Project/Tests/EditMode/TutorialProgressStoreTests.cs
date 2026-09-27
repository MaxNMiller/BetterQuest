using System;
using System.IO;
using NUnit.Framework;
using Spaa.Save;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialProgressStoreTests
    {
        private string tempDirectory;
        private string tutorialPath;

        [SetUp]
        public void SetUp()
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "spaa_tutorial_test_" + Guid.NewGuid());
            tutorialPath = Path.Combine(tempDirectory, SaveLocation.TutorialFileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        private void WriteRaw(string contents)
        {
            Directory.CreateDirectory(tempDirectory);
            File.WriteAllText(tutorialPath, contents);
        }

        [Test]
        public void Load_MissingFile_ReturnsNull()
        {
            var store = new TutorialProgressStore(tutorialPath);

            Assert.IsNull(store.Load());
            Assert.IsFalse(store.LastLoadWasCorrupt);
        }

        [Test]
        public void SaveThenLoad_RoundTrips()
        {
            var store = new TutorialProgressStore(tutorialPath);
            var progress = new TutorialProgress
            {
                version = 3,
                currentStepId = "write-name",
                completed = false,
                skipped = false,
                isReplay = true,
                firstHabitWritten = true
            };
            progress.seenTips.Add("tip-levelup");

            store.Save(progress);
            var loaded = store.Load();

            Assert.AreEqual(3, loaded.version);
            Assert.AreEqual("write-name", loaded.currentStepId);
            Assert.IsFalse(loaded.completed);
            Assert.IsFalse(loaded.skipped);
            Assert.IsTrue(loaded.isReplay);
            Assert.IsTrue(loaded.firstHabitWritten);
            CollectionAssert.AreEqual(new[] { "tip-levelup" }, loaded.seenTips);
            Assert.IsFalse(store.LastLoadWasCorrupt);
        }

        [Test]
        public void SaveCompleted_LoadsCompleted()
        {
            var store = new TutorialProgressStore(tutorialPath);
            var progress = TutorialProgress.CompletedSilently(2);
            progress.skipped = true;

            store.Save(progress);
            var loaded = store.Load();

            Assert.IsTrue(loaded.completed);
            Assert.IsTrue(loaded.skipped);
            Assert.AreEqual(string.Empty, loaded.currentStepId);
        }

        [Test]
        public void Load_CorruptFile_TreatedAsCompleted()
        {
            WriteRaw("{ this is not json");
            var store = new TutorialProgressStore(tutorialPath);

            var loaded = store.Load();

            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.completed);
            Assert.IsTrue(store.LastLoadWasCorrupt);
        }

        [Test]
        public void Load_EmptyFile_TreatedAsCompleted()
        {
            WriteRaw("   ");
            var store = new TutorialProgressStore(tutorialPath);

            Assert.IsTrue(store.Load().completed);
            Assert.IsTrue(store.LastLoadWasCorrupt);
        }

        [Test]
        public void Load_OlderFileMissingFields_GetsDefaults()
        {
            WriteRaw("{\"currentStepId\":\"play\"}");
            var store = new TutorialProgressStore(tutorialPath);

            var loaded = store.Load();

            Assert.AreEqual("play", loaded.currentStepId);
            Assert.IsFalse(loaded.completed);
            Assert.IsNotNull(loaded.seenTips);
            Assert.AreEqual(0, loaded.seenTips.Count);
            Assert.IsFalse(store.LastLoadWasCorrupt);
        }

        [Test]
        public void CorruptFlag_ResetsOnNextGoodLoad()
        {
            WriteRaw("garbage");
            var store = new TutorialProgressStore(tutorialPath);
            store.Load();

            store.Save(TutorialProgress.FirstRun("welcome", 1));
            store.Load();

            Assert.IsFalse(store.LastLoadWasCorrupt);
        }

        [Test]
        public void Save_CreatesMissingDirectory()
        {
            var store = new TutorialProgressStore(tutorialPath);

            store.Save(TutorialProgress.FirstRun("welcome", 1));

            Assert.IsTrue(File.Exists(tutorialPath));
        }

        [Test]
        public void Save_Null_WritesNothing()
        {
            var store = new TutorialProgressStore(tutorialPath);

            store.Save(null);

            Assert.IsFalse(File.Exists(tutorialPath));
        }

        [Test]
        public void Save_NeverTouchesSaveJson()
        {
            Directory.CreateDirectory(tempDirectory);
            string savePath = Path.Combine(tempDirectory, SaveLocation.FileName);
            new JsonSaveService(savePath).Save(new SaveData { currentDayKey = "2026-09-27", monsterSeed = 99 });
            byte[] before = File.ReadAllBytes(savePath);

            var store = new TutorialProgressStore(tutorialPath);
            store.Save(TutorialProgress.FirstRun("welcome", 1));
            store.Save(TutorialProgress.CompletedSilently(1));

            CollectionAssert.AreEqual(before, File.ReadAllBytes(savePath));
        }

        [Test]
        public void TutorialPath_IsSiblingOfSaveFile()
        {
            Assert.AreEqual(Path.GetDirectoryName(SaveLocation.DefaultFilePath), Path.GetDirectoryName(SaveLocation.TutorialFilePath));
            Assert.AreEqual("tutorial.json", Path.GetFileName(SaveLocation.TutorialFilePath));
            Assert.AreNotEqual(SaveLocation.DefaultFilePath, SaveLocation.TutorialFilePath);
        }
    }
}
