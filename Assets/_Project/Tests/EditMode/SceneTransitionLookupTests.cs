using NUnit.Framework;
using Spaa.Flow;

namespace Spaa.Tests.EditMode
{
    public class SceneTransitionLookupTests
    {
        [Test]
        public void TryFindScene_ReturnsMappedSceneName()
        {
            var transitions = new[]
            {
                new SceneTransition { state = GameState.Battle, sceneName = "Battle" }
            };

            bool found = SceneTransitionLookup.TryFindScene(transitions, GameState.Battle, out string sceneName);

            Assert.IsTrue(found);
            Assert.AreEqual("Battle", sceneName);
        }

        [Test]
        public void TryFindScene_UnmappedState_ReturnsFalse()
        {
            var transitions = new[]
            {
                new SceneTransition { state = GameState.Battle, sceneName = "Battle" }
            };

            bool found = SceneTransitionLookup.TryFindScene(transitions, GameState.HabitMenu, out string sceneName);

            Assert.IsFalse(found);
            Assert.IsNull(sceneName);
        }

        [Test]
        public void TryFindScene_NullTransitions_ReturnsFalse()
        {
            bool found = SceneTransitionLookup.TryFindScene(null, GameState.MainMenu, out string sceneName);

            Assert.IsFalse(found);
            Assert.IsNull(sceneName);
        }

        [Test]
        public void TryFindScene_EmptyTransitions_ReturnsFalse()
        {
            bool found = SceneTransitionLookup.TryFindScene(new SceneTransition[0], GameState.MainMenu, out string sceneName);

            Assert.IsFalse(found);
        }
    }
}
