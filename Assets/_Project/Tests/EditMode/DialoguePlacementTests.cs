using NUnit.Framework;
using Spaa.Tutorial;
using UnityEngine;

namespace Spaa.Tests
{
    public class DialoguePlacementTests
    {
        private const float Height = 400f;
        private const float Gap = 24f;
        private static readonly Rect Safe = new Rect(0f, 0f, 1080f, 1920f);

        private static DialoguePlacementResult Place(Rect hole, TutorialDock dock = TutorialDock.Auto, Rect? safe = null)
        {
            return DialoguePlacement.Resolve(hole, true, safe ?? Safe, Height, Gap, dock);
        }

        [Test]
        public void NoHole_Auto_IsCentered()
        {
            var result = DialoguePlacement.Resolve(Rect.zero, false, Safe, Height, Gap, TutorialDock.Auto);

            Assert.AreEqual(760f, result.Top);
            Assert.AreEqual(TutorialArrow.None, result.Arrow);
        }

        [Test]
        public void NoHole_Docked_UsesSafeEdges()
        {
            Assert.AreEqual(Gap, DialoguePlacement.Resolve(Rect.zero, false, Safe, Height, Gap, TutorialDock.Top).Top);
            Assert.AreEqual(1920f - Gap - Height, DialoguePlacement.Resolve(Rect.zero, false, Safe, Height, Gap, TutorialDock.Bottom).Top);
        }

        [Test]
        public void TargetInLowerHalf_GoesAboveWithArrowDown()
        {
            var result = Place(new Rect(0f, 1400f, 1080f, 500f));

            Assert.AreEqual(1400f - Gap - Height, result.Top);
            Assert.AreEqual(TutorialArrow.Down, result.Arrow);
        }

        [Test]
        public void TargetInUpperHalf_GoesBelowWithArrowUp()
        {
            var result = Place(new Rect(40f, 80f, 600f, 200f));

            Assert.AreEqual(280f + Gap, result.Top);
            Assert.AreEqual(TutorialArrow.Up, result.Arrow);
        }

        [Test]
        public void LowerTargetWithoutRoomAbove_GoesBelow()
        {
            var result = Place(new Rect(0f, 300f, 1080f, 1000f));

            Assert.AreEqual(1324f, result.Top);
            Assert.AreEqual(TutorialArrow.Up, result.Arrow);
        }

        [Test]
        public void TallTarget_NoRoomEitherSide_DocksTopWithoutArrow()
        {
            var result = Place(new Rect(0f, 216f, 1080f, 1557f));

            Assert.AreEqual(Gap, result.Top);
            Assert.AreEqual(TutorialArrow.None, result.Arrow);
        }

        [Test]
        public void TopDock_IsKeptWhenClearOfTarget()
        {
            var result = Place(new Rect(110f, 600f, 860f, 112f), TutorialDock.Top);

            Assert.AreEqual(Gap, result.Top);
            Assert.AreEqual(TutorialArrow.Down, result.Arrow);
        }

        [Test]
        public void TopDock_ThatWouldCoverTarget_MovesToAClearSide()
        {
            var result = Place(new Rect(60f, 130f, 960f, 1180f), TutorialDock.Top);

            Assert.AreEqual(1310f + Gap, result.Top);
            Assert.AreEqual(TutorialArrow.Up, result.Arrow);
        }

        [Test]
        public void BottomDock_IsKeptWhenClear_AndLeftWhenItWouldCover()
        {
            var kept = Place(new Rect(0f, 200f, 1080f, 200f), TutorialDock.Bottom);
            var moved = Place(new Rect(0f, 1500f, 1080f, 200f), TutorialDock.Bottom);

            Assert.AreEqual(1920f - Gap - Height, kept.Top);
            Assert.AreEqual(TutorialArrow.Up, kept.Arrow);
            Assert.AreEqual(1500f - Gap - Height, moved.Top);
            Assert.AreEqual(TutorialArrow.Down, moved.Arrow);
        }

        [Test]
        public void NothingFits_DocksOnTheRoomierSide()
        {
            var result = Place(new Rect(0f, 100f, 1080f, 1400f));

            Assert.AreEqual(1920f - Gap - Height, result.Top);
            Assert.AreEqual(TutorialArrow.None, result.Arrow);
            Assert.IsTrue(result.CoversHole);
        }

        [Test]
        public void CoversHole_OnlyWhenNothingFits()
        {
            Assert.IsFalse(Place(new Rect(0f, 1400f, 1080f, 500f)).CoversHole);
            Assert.IsFalse(Place(new Rect(40f, 80f, 600f, 200f)).CoversHole);
            Assert.IsFalse(Place(new Rect(110f, 600f, 860f, 112f), TutorialDock.Top).CoversHole);
            Assert.IsFalse(DialoguePlacement.Resolve(Rect.zero, false, Safe, Height, Gap, TutorialDock.Auto).CoversHole);
            Assert.IsTrue(Place(new Rect(0f, 216f, 1080f, 1557f)).CoversHole);
        }

        [Test]
        public void Arrow_PointsAtTargetCentre()
        {
            Assert.AreEqual(540f - 32f - 22f, DialoguePlacement.ArrowX(540f, 32f, 1016f, 44f, 56f));
        }

        [Test]
        public void Arrow_StaysClearOfRoundedCorners()
        {
            Assert.AreEqual(56f, DialoguePlacement.ArrowX(0f, 32f, 1016f, 44f, 56f));
            Assert.AreEqual(1016f - 56f - 44f, DialoguePlacement.ArrowX(1080f, 32f, 1016f, 44f, 56f));
        }

        [Test]
        public void Placement_StaysInsideNotchedSafeArea()
        {
            var notched = new Rect(0f, 132f, 1080f, 1720f);

            var docked = Place(new Rect(0f, 216f, 1080f, 1557f), TutorialDock.Auto, notched);
            var above = Place(new Rect(0f, 1400f, 1080f, 400f), TutorialDock.Auto, notched);

            Assert.AreEqual(132f + Gap, docked.Top);
            Assert.GreaterOrEqual(above.Top, notched.yMin + Gap);
            Assert.LessOrEqual(above.Top + Height, notched.yMax - Gap);
        }
    }
}
