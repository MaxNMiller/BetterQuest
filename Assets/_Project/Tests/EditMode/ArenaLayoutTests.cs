using System;
using NUnit.Framework;
using Spaa.Battle;
using Spaa.Elements;

namespace Spaa.Tests
{
    public class ArenaLayoutTests
    {
        private static readonly Element[] AllElements =
            (Element[])Enum.GetValues(typeof(Element));

        [Test]
        public void EveryElement_ReturnsNonEmptyLayout()
        {
            foreach (var element in AllElements)
            {
                var props = ArenaLayout.GetProps(element);

                Assert.IsNotEmpty(props, $"{element} should have at least one prop.");
            }
        }

        [Test]
        public void SameElement_ReturnsDeterministicLayout()
        {
            var first = ArenaLayout.GetProps(Element.Food);
            var second = ArenaLayout.GetProps(Element.Food);

            Assert.AreEqual(first.Length, second.Length);
            for (int i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i].Primitive, second[i].Primitive);
                Assert.AreEqual(first[i].LocalPosition, second[i].LocalPosition);
                Assert.AreEqual(first[i].LocalScale, second[i].LocalScale);
            }
        }

        [Test]
        public void DifferentElements_ReturnDifferentLayouts()
        {
            var rest = ArenaLayout.GetProps(Element.Rest);
            var move = ArenaLayout.GetProps(Element.Move);

            Assert.AreNotEqual(rest.Length, move.Length);
        }

        [Test]
        public void AllProps_HaveNonZeroScale()
        {
            foreach (var element in AllElements)
            {
                foreach (var prop in ArenaLayout.GetProps(element))
                {
                    Assert.Greater(prop.LocalScale.x, 0f);
                    Assert.Greater(prop.LocalScale.y, 0f);
                    Assert.Greater(prop.LocalScale.z, 0f);
                }
            }
        }
    }
}
