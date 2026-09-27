using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Spaa.Elements;

namespace Spaa.Tests
{
    public class ElementDefinitionTests
    {
        [Test]
        public void Element_Enum_HasExactlyFiveValues()
        {
            var values = (Element[])Enum.GetValues(typeof(Element));

            Assert.AreEqual(5, values.Length);
        }

        [Test]
        public void ElementDefinitions_ExistForEveryElement_WithSpecColors()
        {
            var expectedHex = new Dictionary<Element, string>
            {
                { Element.Rest, "#A733D6" },
                { Element.SelfCare, "#3384D6" },
                { Element.Food, "#239C26" },
                { Element.Rebuild, "#E7D225" },
                { Element.Move, "#D63333" },
            };

            var guids = AssetDatabase.FindAssets("t:ElementDefinition");
            Assert.AreEqual(5, guids.Length, "Expected exactly 5 ElementDefinition assets.");

            var foundElements = new HashSet<Element>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var definition = AssetDatabase.LoadAssetAtPath<ElementDefinition>(path);
                Assert.IsNotNull(definition, $"Failed to load ElementDefinition at {path}");
                Assert.IsTrue(foundElements.Add(definition.Element), $"Duplicate ElementDefinition for {definition.Element}");

                ColorUtility.TryParseHtmlString(expectedHex[definition.Element], out var expectedColor);
                AssertColorsApproximatelyEqual(expectedColor, definition.Color, definition.Element);
            }

            Assert.AreEqual(5, foundElements.Count);
        }

        private static void AssertColorsApproximatelyEqual(Color expected, Color actual, Element element)
        {
            const float tolerance = 0.01f;
            Assert.AreEqual(expected.r, actual.r, tolerance, $"{element} red channel");
            Assert.AreEqual(expected.g, actual.g, tolerance, $"{element} green channel");
            Assert.AreEqual(expected.b, actual.b, tolerance, $"{element} blue channel");
        }
    }
}
