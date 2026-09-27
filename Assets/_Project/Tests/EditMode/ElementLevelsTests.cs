using System;
using NUnit.Framework;
using Spaa.Elements;
using Spaa.Progression;

namespace Spaa.Tests
{
    public class ElementLevelsTests
    {
        [Test]
        public void AllElements_StartAtLevelOne()
        {
            var levels = new ElementLevels();

            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                Assert.AreEqual(1, levels.GetLevel(element));
            }
        }

        [Test]
        public void IncrementLevel_IncrementsOnlyThatElement()
        {
            var levels = new ElementLevels();

            levels.IncrementLevel(Element.Move);

            Assert.AreEqual(2, levels.GetLevel(Element.Move));
            Assert.AreEqual(1, levels.GetLevel(Element.Rest));
            Assert.AreEqual(1, levels.GetLevel(Element.SelfCare));
            Assert.AreEqual(1, levels.GetLevel(Element.Food));
            Assert.AreEqual(1, levels.GetLevel(Element.Rebuild));
        }
    }
}
