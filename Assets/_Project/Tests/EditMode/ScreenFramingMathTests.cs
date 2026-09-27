using NUnit.Framework;
using Spaa.UI;

namespace Spaa.Tests
{
    public class ScreenFramingMathTests
    {
        [Test]
        public void EqualCover_KeepsTargetCentred()
        {
            Assert.AreEqual(0.5f, ScreenFramingMath.BandCentreFromTop(0.2f, 0.2f), 1e-5f);
            Assert.AreEqual(0f, ScreenFramingMath.ComposerY(0.2f, 0.2f, 0.4f), 1e-5f);
        }

        [Test]
        public void TallBottomSheet_MovesTargetUp()
        {
            Assert.AreEqual(0.4f, ScreenFramingMath.BandCentreFromTop(0.1f, 0.3f), 1e-5f);
            Assert.AreEqual(-0.1f, ScreenFramingMath.ComposerY(0.1f, 0.3f, 0.4f), 1e-5f);
        }

        [Test]
        public void Offset_IsClampedToLimit()
        {
            Assert.AreEqual(-0.15f, ScreenFramingMath.ComposerY(0f, 0.8f, 0.15f), 1e-5f);
            Assert.AreEqual(0.15f, ScreenFramingMath.ComposerY(0.8f, 0f, -0.15f), 1e-5f);
        }

        [Test]
        public void OverlappingCover_FallsBackToCentre()
        {
            Assert.AreEqual(0.5f, ScreenFramingMath.BandCentreFromTop(0.6f, 0.6f), 1e-5f);
            Assert.AreEqual(0f, ScreenFramingMath.ComposerY(0.6f, 0.6f, 0.4f), 1e-5f);
        }

        [Test]
        public void OutOfRangeInputs_AreClamped()
        {
            Assert.AreEqual(0.5f, ScreenFramingMath.BandCentreFromTop(-1f, -1f), 1e-5f);
        }
    }
}
