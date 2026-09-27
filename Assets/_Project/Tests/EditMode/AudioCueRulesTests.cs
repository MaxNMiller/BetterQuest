using NUnit.Framework;
using Spaa.Audio;
using Spaa.Battle;
using Spaa.Flow;

namespace Spaa.Tests
{
    public class AudioCueRulesTests
    {
        [Test]
        public void LevelUpState_PlaysLevelUp()
        {
            Assert.AreEqual(SfxCue.LevelUp, AudioCueRules.ForState(GameState.LevelUp));
        }

        [TestCase(GameState.SoftReminder)]
        [TestCase(GameState.Dashboard)]
        public void OverlayStates_PlayPopup(GameState state)
        {
            Assert.AreEqual(SfxCue.Popup, AudioCueRules.ForState(state));
        }

        [TestCase(GameState.Splash)]
        [TestCase(GameState.MainMenu)]
        [TestCase(GameState.HabitMenu)]
        [TestCase(GameState.Battle)]
        public void OtherStates_AreSilent(GameState state)
        {
            Assert.AreEqual(SfxCue.None, AudioCueRules.ForState(state));
        }

        [Test]
        public void SuccessfulAttack_PlaysAttack()
        {
            var result = new CommandResult(true, 10, false, false, "hit");
            Assert.AreEqual(SfxCue.Attack, AudioCueRules.ForAttack(result));
        }

        [Test]
        public void FailedAttack_IsSilent()
        {
            Assert.AreEqual(SfxCue.None, AudioCueRules.ForAttack(CommandResult.Failed("nope")));
        }
    }
}
