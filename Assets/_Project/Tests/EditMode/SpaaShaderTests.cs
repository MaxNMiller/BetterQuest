using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Spaa.Tests
{
    public class SpaaShaderTests
    {
        private static readonly string[] ShaderNames =
        {
            "Spaa/Toon",
            "Spaa/Ground",
            "Spaa/SkyGradient",
            "Spaa/Particle",
        };

        private static readonly (string path, string shader)[] Materials =
        {
            ("Assets/_Project/Art/Materials/M_Toon.mat", "Spaa/Toon"),
            ("Assets/_Project/Art/Materials/M_Ground.mat", "Spaa/Ground"),
            ("Assets/_Project/Art/Materials/M_Sky.mat", "Spaa/SkyGradient"),
            ("Assets/_Project/Art/Materials/M_Particle.mat", "Spaa/Particle"),
        };

        [Test]
        public void SpaaShaders_CompileWithoutErrors()
        {
            foreach (var shaderName in ShaderNames)
            {
                var shader = Shader.Find(shaderName);
                Assert.IsNotNull(shader, shaderName + " not found");
                Assert.IsFalse(ShaderUtil.ShaderHasError(shader), shaderName + " has compile errors");
                Assert.IsTrue(shader.isSupported, shaderName + " is not supported on this platform");
            }
        }

        [Test]
        public void M4Materials_ExistAndUseSpaaShaders()
        {
            foreach (var (path, shaderName) in Materials)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                Assert.IsNotNull(material, path + " missing");
                Assert.AreEqual(shaderName, material.shader.name, path);
            }
        }

        [Test]
        public void ToonShader_ExposesFlashAndDissolve()
        {
            var shader = Shader.Find("Spaa/Toon");
            Assert.GreaterOrEqual(shader.FindPropertyIndex("_Flash"), 0);
            Assert.GreaterOrEqual(shader.FindPropertyIndex("_Dissolve"), 0);
            Assert.GreaterOrEqual(shader.FindPropertyIndex("_BaseColor"), 0);
        }

        [Test]
        public void ToonShader_SupportsTexturedAndTransparentMaterials()
        {
            var shader = Shader.Find("Spaa/Toon");
            foreach (var property in new[] { "_BaseMap", "_TextureTint", "_SrcBlend", "_DstBlend", "_ZWrite" })
            {
                Assert.GreaterOrEqual(shader.FindPropertyIndex(property), 0, property);
            }
        }

        [Test]
        public void GloobToonMaterials_KeepAuthoredTexturesAndEffects()
        {
            foreach (var name in new[] { "M_Gloob_Toon", "M_GloobEye_Toon", "M_GloobMouth_Toon" })
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/" + name + ".mat");
                Assert.IsNotNull(material, name + " missing (run Tools/Spaa/Blockout/1)");
                Assert.AreEqual("Spaa/Toon", material.shader.name, name);
                Assert.IsNotNull(material.GetTexture("_BaseMap"), name + " lost its authored texture");
            }

            var body = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Gloob_Toon.mat");
            Assert.AreEqual(1f, body.GetFloat("_TextureTint"), "body must take the element tint");
            Assert.Less(body.renderQueue, 2500, "body renders opaque");

            var eye = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_GloobEye_Toon.mat");
            Assert.GreaterOrEqual(eye.renderQueue, 3000, "eyes keep their transparency");
            Assert.AreEqual(0f, eye.GetFloat("_ZWrite"));
        }
    }
}
