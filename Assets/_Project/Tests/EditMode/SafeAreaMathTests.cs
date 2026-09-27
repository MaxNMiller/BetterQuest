using NUnit.Framework;
using UnityEngine;
using Spaa.UI;

namespace Spaa.Tests
{
    public class SafeAreaMathTests
    {
        [Test]
        public void FullBleed_NoNotch_ReturnsZeroMargins()
        {
            var safeArea = new Rect(0, 0, 1080, 1920);

            var margins = SafeAreaMath.ComputeMargins(safeArea, 1080, 1920);

            Assert.AreEqual(0f, margins.Left);
            Assert.AreEqual(0f, margins.Right);
            Assert.AreEqual(0f, margins.Top);
            Assert.AreEqual(0f, margins.Bottom);
        }

        [Test]
        public void NotchAtTop_ReturnsTopMargin()
        {
            var safeArea = new Rect(0, 0, 1080, 1850);

            var margins = SafeAreaMath.ComputeMargins(safeArea, 1080, 1920);

            Assert.AreEqual(70f, margins.Top, 0.001f);
            Assert.AreEqual(0f, margins.Bottom);
            Assert.AreEqual(0f, margins.Left);
            Assert.AreEqual(0f, margins.Right);
        }

        [Test]
        public void HomeIndicatorAtBottom_ReturnsBottomMargin()
        {
            var safeArea = new Rect(0, 40, 1080, 1880);

            var margins = SafeAreaMath.ComputeMargins(safeArea, 1080, 1920);

            Assert.AreEqual(40f, margins.Bottom, 0.001f);
            Assert.AreEqual(0f, margins.Top, 0.001f);
        }

        [Test]
        public void SideNotches_ReturnLeftAndRightMargins()
        {
            var safeArea = new Rect(30, 0, 1000, 1920);

            var margins = SafeAreaMath.ComputeMargins(safeArea, 1080, 1920);

            Assert.AreEqual(30f, margins.Left, 0.001f);
            Assert.AreEqual(50f, margins.Right, 0.001f);
        }
    }
}
