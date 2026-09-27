using System;
using NUnit.Framework;
using Spaa.Audio;

namespace Spaa.Tests
{
    public class ClipPickerTests
    {
        [Test]
        public void NoClips_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, new ClipPicker(new Random(1)).Next(0));
        }

        [Test]
        public void SingleClip_AlwaysZero()
        {
            var picker = new ClipPicker(new Random(1));
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(0, picker.Next(1));
            }
        }

        [Test]
        public void ThreeClips_NeverRepeatBackToBack_AndStayInRange()
        {
            var picker = new ClipPicker(new Random(42));
            int previous = -1;
            var seen = new bool[3];
            for (int i = 0; i < 200; i++)
            {
                int index = picker.Next(3);
                Assert.That(index, Is.InRange(0, 2));
                Assert.AreNotEqual(previous, index);
                seen[index] = true;
                previous = index;
            }

            Assert.IsTrue(seen[0] && seen[1] && seen[2]);
        }

        [Test]
        public void SameSeed_SameSequence()
        {
            var a = new ClipPicker(new Random(7));
            var b = new ClipPicker(new Random(7));
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(a.Next(3), b.Next(3));
            }
        }

        [Test]
        public void NullRandom_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ClipPicker(null));
        }
    }
}
