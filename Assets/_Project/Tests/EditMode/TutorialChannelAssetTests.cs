using NUnit.Framework;
using Spaa.Events;
using UnityEditor;

namespace Spaa.Tests
{
    public class TutorialChannelAssetTests
    {
        [TestCase("Assets/_Project/Data/Events/OnHabitSaved.asset")]
        [TestCase("Assets/_Project/Data/Events/OnHabitEditorOpened.asset")]
        [TestCase("Assets/_Project/Data/Events/OnHabitEditorClosed.asset")]
        public void HabitEditorChannel_Exists(string path)
        {
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<VoidEventChannelSO>(path),
                $"Missing {path}. Run Tools/Spaa/Tutorial/Create Assets.");
        }

        [Test]
        public void HabitEditorChannels_AreDistinctAssets()
        {
            var saved = AssetDatabase.LoadAssetAtPath<VoidEventChannelSO>("Assets/_Project/Data/Events/OnHabitSaved.asset");
            var opened = AssetDatabase.LoadAssetAtPath<VoidEventChannelSO>("Assets/_Project/Data/Events/OnHabitEditorOpened.asset");
            var closed = AssetDatabase.LoadAssetAtPath<VoidEventChannelSO>("Assets/_Project/Data/Events/OnHabitEditorClosed.asset");

            Assert.AreNotSame(saved, opened);
            Assert.AreNotSame(saved, closed);
            Assert.AreNotSame(opened, closed);
        }
    }
}
