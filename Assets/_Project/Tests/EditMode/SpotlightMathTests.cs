using NUnit.Framework;
using Spaa.Tutorial;
using UnityEngine;

namespace Spaa.Tests
{
    public class SpotlightMathTests
    {
        private static readonly Vector2 Panel = new Vector2(1080f, 1920f);

        private static Rect[] Compute(Rect target, float padding, out bool hasHole)
        {
            var dims = new Rect[SpotlightMath.RectCount];
            hasHole = SpotlightMath.Compute(target, Panel, padding, dims);
            return dims;
        }

        [Test]
        public void Target_ProducesFourRectsAroundPaddedHole()
        {
            var dims = Compute(new Rect(24f, 1463f, 506f, 143f), 12f, out bool hasHole);

            Assert.IsTrue(hasHole);
            Assert.AreEqual(new Rect(0f, 0f, 1080f, 1451f), dims[0]);
            Assert.AreEqual(new Rect(0f, 1618f, 1080f, 302f), dims[1]);
            Assert.AreEqual(new Rect(0f, 1451f, 12f, 167f), dims[2]);
            Assert.AreEqual(new Rect(542f, 1451f, 538f, 167f), dims[3]);
        }

        [Test]
        public void DimRectsPlusHole_CoverThePanelExactly()
        {
            var target = new Rect(300f, 700f, 200f, 90f);
            var dims = Compute(target, 16f, out _);
            var hole = SpotlightMath.PaddedHole(target, Panel, 16f);

            float area = hole.width * hole.height;
            foreach (var rect in dims)
            {
                area += rect.width * rect.height;
            }

            Assert.AreEqual(Panel.x * Panel.y, area, 0.01f);
        }

        [Test]
        public void TargetAtEdge_IsClampedToPanel()
        {
            var hole = SpotlightMath.PaddedHole(new Rect(0f, 0f, 100f, 100f), Panel, 12f);
            var dims = Compute(new Rect(0f, 0f, 100f, 100f), 12f, out bool hasHole);

            Assert.IsTrue(hasHole);
            Assert.AreEqual(new Rect(0f, 0f, 112f, 112f), hole);
            Assert.AreEqual(0f, dims[0].height);
            Assert.AreEqual(0f, dims[2].width);
        }

        [Test]
        public void NaNBounds_FallBackToSingleFullRect()
        {
            var dims = Compute(new Rect(0f, 0f, float.NaN, float.NaN), 12f, out bool hasHole);

            Assert.IsFalse(hasHole);
            Assert.AreEqual(new Rect(0f, 0f, 1080f, 1920f), dims[0]);
            Assert.AreEqual(Rect.zero, dims[1]);
            Assert.AreEqual(Rect.zero, dims[2]);
            Assert.AreEqual(Rect.zero, dims[3]);
        }

        [Test]
        public void ZeroSizedOrOffPanelTarget_HasNoHole()
        {
            Compute(new Rect(500f, 500f, 0f, 0f), 12f, out bool zeroSized);
            Compute(new Rect(2000f, 3000f, 100f, 100f), 12f, out bool offPanel);

            Assert.IsFalse(zeroSized);
            Assert.IsFalse(offPanel);
        }
    }
}
