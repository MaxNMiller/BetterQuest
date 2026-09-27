using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Spaa.Battle;
using Spaa.Elements;

namespace Spaa.Tests
{
    public class MonsterPresentationTests
    {
        private const string FaceLibraryPath = "Assets/_Project/Data/Monsters/GloobFaceLibrary.asset";

        [TestCase(10, 10, false, MonsterExpression.Neutral)]
        [TestCase(4, 10, false, MonsterExpression.Neutral)]
        [TestCase(3, 10, false, MonsterExpression.LowHp)]
        [TestCase(1, 10, false, MonsterExpression.LowHp)]
        [TestCase(8, 10, true, MonsterExpression.Hurt)]
        [TestCase(2, 10, true, MonsterExpression.Hurt)]
        [TestCase(0, 10, true, MonsterExpression.Defeated)]
        [TestCase(-5, 10, false, MonsterExpression.Defeated)]
        [TestCase(1, 0, false, MonsterExpression.Neutral)]
        public void ExpressionRules_PriorityDefeatedHurtLowHpNeutral(int hp, int maxHp, bool hurt, MonsterExpression expected)
        {
            Assert.AreEqual(expected, MonsterExpressionRules.Resolve(hp, maxHp, hurt, 0.3f));
        }

        [Test]
        public void FaceLayer_MissingOverridePartsKeepBaseFace()
        {
            var baseEye = new Texture2D(1, 1);
            var baseMouth = new Texture2D(1, 1);
            var lowEye = new Texture2D(1, 1);
            try
            {
                var layered = MonsterFace.Layer(new MonsterFace(baseEye, baseMouth), new MonsterFace(lowEye, null));
                Assert.AreSame(lowEye, layered.Eye);
                Assert.AreSame(baseMouth, layered.Mouth);

                var untouched = MonsterFace.Layer(new MonsterFace(baseEye, baseMouth), default);
                Assert.AreSame(baseEye, untouched.Eye);
                Assert.AreSame(baseMouth, untouched.Mouth);
            }
            finally
            {
                Object.DestroyImmediate(baseEye);
                Object.DestroyImmediate(baseMouth);
                Object.DestroyImmediate(lowEye);
            }
        }

        [Test]
        public void FaceLibrary_EmptyLibraryReturnsEmptyFaceWithoutThrowing()
        {
            var library = ScriptableObject.CreateInstance<MonsterFaceLibrary>();
            try
            {
                var face = library.GetFace(MonsterExpression.LowHp, Element.Move);
                Assert.IsNull(face.Eye);
                Assert.IsNull(face.Mouth);
            }
            finally
            {
                Object.DestroyImmediate(library);
            }
        }

        [TestCase(Element.Rest, "Rest")]
        [TestCase(Element.SelfCare, "Self")]
        [TestCase(Element.Food, "Nut")]
        [TestCase(Element.Rebuild, "Rebuild")]
        [TestCase(Element.Move, "Move")]
        public void GloobFaceLibrary_MapsEachElementToItsArt(Element element, string prefix)
        {
            var library = AssetDatabase.LoadAssetAtPath<MonsterFaceLibrary>(FaceLibraryPath);
            Assert.IsNotNull(library, $"Missing {FaceLibraryPath}; run Tools/Spaa/Monster/Wire Gloob Animations and Faces.");

            var face = library.GetFace(MonsterExpression.Neutral, element);
            Assert.AreEqual(prefix + "_Eye", face.Eye != null ? face.Eye.name : null);
            Assert.AreEqual(prefix + "_Mouth", face.Mouth != null ? face.Mouth.name : null);
        }

        [Test]
        public void GloobFaceLibrary_ExpressionsUseStateArtAndLowHpKeepsElementMouth()
        {
            var library = AssetDatabase.LoadAssetAtPath<MonsterFaceLibrary>(FaceLibraryPath);
            Assert.IsNotNull(library);

            var hurt = library.GetFace(MonsterExpression.Hurt, Element.Rest);
            Assert.AreEqual("TakeDamage_Eye", hurt.Eye.name);
            Assert.AreEqual("TakeDamage_Mouth", hurt.Mouth.name);

            var low = library.GetFace(MonsterExpression.LowHp, Element.Food);
            Assert.AreEqual("LowHP_Eye", low.Eye.name);
            Assert.AreEqual("Nut_Mouth", low.Mouth.name);

            var dead = library.GetFace(MonsterExpression.Defeated, Element.Move);
            Assert.AreEqual("Death_Eye", dead.Eye.name);
            Assert.AreEqual("Death_Mouth", dead.Mouth.name);
        }

        [Test]
        public void OneShotWeight_FadesInPlaysAndFadesOut()
        {
            Assert.AreEqual(0f, MonsterAnimationBlend.OneShotWeight(-0.1f, 1f, 0.1f, false));
            Assert.AreEqual(0.5f, MonsterAnimationBlend.OneShotWeight(0.05f, 1f, 0.1f, false), 1e-4f);
            Assert.AreEqual(1f, MonsterAnimationBlend.OneShotWeight(0.5f, 1f, 0.1f, false));
            Assert.AreEqual(0.5f, MonsterAnimationBlend.OneShotWeight(0.95f, 1f, 0.1f, false), 1e-4f);
            Assert.AreEqual(0f, MonsterAnimationBlend.OneShotWeight(1.2f, 1f, 0.1f, false));
        }

        [Test]
        public void OneShotWeight_HoldLastFrameStaysAtFullWeight()
        {
            Assert.AreEqual(1f, MonsterAnimationBlend.OneShotWeight(5f, 1f, 0.1f, true));
            Assert.IsFalse(MonsterAnimationBlend.IsFinished(5f, 1f, true));
            Assert.IsTrue(MonsterAnimationBlend.IsFinished(1f, 1f, false));
            Assert.IsFalse(MonsterAnimationBlend.IsFinished(0.99f, 1f, false));
        }

        [Test]
        public void OneShotWeight_ShortClipNeverExceedsOne()
        {
            float mid = MonsterAnimationBlend.OneShotWeight(0.05f, 0.1f, 0.1f, false);
            Assert.That(mid, Is.InRange(0f, 1f));
            Assert.AreEqual(1f, MonsterAnimationBlend.OneShotWeight(0.2f, 1f, 0f, false));
        }

        [Test]
        public void ControlRigFollow_AtRestReturnsTargetRest()
        {
            var sourceRest = Matrix4x4.TRS(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 270f, 0f), Vector3.one);
            var targetRest = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

            var pose = ControlRigFollower.ComputeFollowPose(sourceRest, sourceRest.inverse, targetRest);

            AssertMatrixApprox(targetRest, pose);
        }

        [Test]
        public void ControlRigFollow_AppliesSourceDeltaToTarget()
        {
            var sourceRest = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 270f, 0f), Vector3.one);
            var sourceNow = Matrix4x4.Translate(new Vector3(0f, 5f, 0f)) * sourceRest * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 90f));
            var targetRest = Matrix4x4.Translate(new Vector3(0f, 1f, 0f));

            var pose = ControlRigFollower.ComputeFollowPose(sourceNow, sourceRest.inverse, targetRest);

            Vector3 expected = sourceNow.MultiplyPoint3x4(sourceRest.inverse.MultiplyPoint3x4(new Vector3(0f, 1f, 0f)));
            Vector3 actual = pose.GetColumn(3);
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(1e-4f));
            Assert.That(actual.y, Is.GreaterThan(4.9f));
        }

        private static void AssertMatrixApprox(Matrix4x4 expected, Matrix4x4 actual)
        {
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(expected[i], actual[i], 1e-4f, $"element {i}");
            }
        }
    }
}
