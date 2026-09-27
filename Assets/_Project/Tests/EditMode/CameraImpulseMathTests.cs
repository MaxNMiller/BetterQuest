using NUnit.Framework;
using Spaa.Battle;

namespace Spaa.Tests
{
    public class CameraImpulseMathTests
    {
        [Test]
        public void ForceFor_WeaknessHitShakesHarderThanNeutral()
        {
            var neutral = new CommandResult(true, 10, false, false, "");
            var weak = new CommandResult(true, 20, true, false, "");

            Assert.Greater(CameraImpulseMath.ForceFor(weak), CameraImpulseMath.ForceFor(neutral));
            Assert.Greater(CameraImpulseMath.ForceFor(neutral), 0f);
        }

        [Test]
        public void ForceFor_DefeatingBlowIsStrongest()
        {
            var weakKill = new CommandResult(true, 20, true, true, "");
            var weak = new CommandResult(true, 20, true, false, "");

            Assert.Greater(CameraImpulseMath.ForceFor(weakKill), CameraImpulseMath.ForceFor(weak));
        }

        [Test]
        public void ForceFor_FailedCommand_NoShake()
        {
            Assert.AreEqual(0f, CameraImpulseMath.ForceFor(CommandResult.Failed("exhausted")));
        }
    }
}
