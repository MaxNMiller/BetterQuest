using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Elements;
using Spaa.VFX;

namespace Spaa.Tests
{
    public class ElementVfxProfilesTests
    {
        [Test]
        public void For_EveryElement_HasDistinctLook()
        {
            var looks = new HashSet<(int shape, bool stretched)>();
            foreach (Element element in System.Enum.GetValues(typeof(Element)))
            {
                var profile = ElementVfxProfiles.For(element);
                Assert.IsTrue(looks.Add((profile.Shape, profile.Stretched)), $"{element} shares a look with another element");
            }
        }

        [Test]
        public void EmitCount_WeaknessEmitsMoreThanNeutral()
        {
            foreach (Element element in System.Enum.GetValues(typeof(Element)))
            {
                var profile = ElementVfxProfiles.For(element);
                Assert.Greater(ElementVfxProfiles.EmitCount(profile, true), ElementVfxProfiles.EmitCount(profile, false), element.ToString());
            }
        }

        [Test]
        public void EmitCount_NeverExceedsMobileBudget()
        {
            foreach (Element element in System.Enum.GetValues(typeof(Element)))
            {
                var profile = ElementVfxProfiles.For(element);
                Assert.LessOrEqual(ElementVfxProfiles.EmitCount(profile, true), ElementVfxProfiles.MaxParticlesPerEffect, element.ToString());
            }

            var huge = new ElementVfxProfile(ElementVfxProfile.ShapeOrb, 500, 1f, 1f, 1f, 0f, 1f);
            Assert.AreEqual(ElementVfxProfiles.MaxParticlesPerEffect, ElementVfxProfiles.EmitCount(huge, true));
        }

        [Test]
        public void For_EveryElement_HasPositiveTuning()
        {
            foreach (Element element in System.Enum.GetValues(typeof(Element)))
            {
                var profile = ElementVfxProfiles.For(element);
                Assert.Greater(profile.BurstCount, 0);
                Assert.Greater(profile.Lifetime, 0f);
                Assert.Greater(profile.Size, 0f);
            }
        }
    }
}
