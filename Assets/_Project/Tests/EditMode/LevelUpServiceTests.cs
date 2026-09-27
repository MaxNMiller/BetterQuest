using System;
using System.IO;
using NUnit.Framework;
using Spaa.Elements;
using Spaa.Progression;
using Spaa.Save;

namespace Spaa.Tests
{
    public class LevelUpServiceTests
    {
        [Test]
        public void ApplyUpgrade_IncrementsOnlyChosenElement()
        {
            var levels = new ElementLevels();
            var service = new LevelUpService();

            service.ApplyUpgrade(levels, Element.Rebuild);

            Assert.AreEqual(2, levels.GetLevel(Element.Rebuild));
            Assert.AreEqual(1, levels.GetLevel(Element.Rest));
            Assert.AreEqual(1, levels.GetLevel(Element.Move));
        }

        [Test]
        public void SaveToThenLoadFrom_RoundTripsLevels()
        {
            var levels = new ElementLevels();
            var service = new LevelUpService();
            service.ApplyUpgrade(levels, Element.Food);
            service.ApplyUpgrade(levels, Element.Food);

            var data = new SaveData();
            service.SaveTo(data, levels);
            var loaded = service.LoadFrom(data);

            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                Assert.AreEqual(levels.GetLevel(element), loaded.GetLevel(element));
            }
            Assert.AreEqual(3, loaded.GetLevel(Element.Food));
        }

        [Test]
        public void Levels_PersistThroughJsonSaveService()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "spaa_levelup_test_" + Guid.NewGuid() + ".json");
            try
            {
                var levels = new ElementLevels();
                var service = new LevelUpService();
                service.ApplyUpgrade(levels, Element.SelfCare);

                var saveService = new JsonSaveService(tempFilePath);
                var data = saveService.Load();
                service.SaveTo(data, levels);
                saveService.Save(data);

                var reloadedData = saveService.Load();
                var reloadedLevels = service.LoadFrom(reloadedData);

                Assert.AreEqual(2, reloadedLevels.GetLevel(Element.SelfCare));
                Assert.AreEqual(1, reloadedLevels.GetLevel(Element.Rest));
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
    }
}
