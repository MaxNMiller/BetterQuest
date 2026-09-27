using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UIElements;
using Spaa.Elements;
using Spaa.UI;

namespace Spaa.Tests
{
    public class ElementStyleTests
    {
        [Test]
        public void EveryElement_MapsToUniqueClass()
        {
            var seen = new HashSet<string>();
            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                string cssClass = ElementStyle.ClassFor(element);
                StringAssert.StartsWith("el--", cssClass);
                Assert.IsTrue(seen.Add(cssClass), $"duplicate class {cssClass}");
            }

            Assert.AreEqual(Enum.GetValues(typeof(Element)).Length, ElementStyle.ClassCount);
        }

        [Test]
        public void ClassFor_MatchesThemeNames()
        {
            Assert.AreEqual("el--rest", ElementStyle.ClassFor(Element.Rest));
            Assert.AreEqual("el--selfcare", ElementStyle.ClassFor(Element.SelfCare));
            Assert.AreEqual("el--food", ElementStyle.ClassFor(Element.Food));
            Assert.AreEqual("el--rebuild", ElementStyle.ClassFor(Element.Rebuild));
            Assert.AreEqual("el--move", ElementStyle.ClassFor(Element.Move));
        }

        [Test]
        public void Apply_RemovesStaleElementClass_KeepsOthers()
        {
            var element = new VisualElement();
            element.AddToClassList("attack-slot");

            ElementStyle.Apply(element, Element.Rest);
            ElementStyle.Apply(element, Element.Move);

            Assert.IsTrue(element.ClassListContains("el--move"));
            Assert.IsFalse(element.ClassListContains("el--rest"));
            Assert.IsTrue(element.ClassListContains("attack-slot"));
        }

        [Test]
        public void Apply_IsIdempotent()
        {
            var element = new VisualElement();

            ElementStyle.Apply(element, Element.Food);
            ElementStyle.Apply(element, Element.Food);

            int count = 0;
            foreach (var cssClass in element.GetClasses())
            {
                if (cssClass.StartsWith("el--", StringComparison.Ordinal))
                {
                    count++;
                }
            }

            Assert.AreEqual(1, count);
        }
    }
}
