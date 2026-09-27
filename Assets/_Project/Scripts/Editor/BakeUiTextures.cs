using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spaa.Editor
{
    public static class BakeUiTextures
    {
        private const string Folder = "Assets/_Project/UI/Textures";
        private const string FadeEdgePath = Folder + "/T_FadeEdge.png";
        private const string VignettePath = Folder + "/T_Vignette.png";

        [MenuItem("Tools/Spaa/UI/Bake UI Textures")]
        public static void Bake()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets/_Project/UI", "Textures");
            }

            Write(FadeEdgePath, 8, 128, (x, y, w, h) =>
            {
                float t = 1f - (float)y / (h - 1);
                return new Color(1f, 1f, 1f, Mathf.SmoothStep(0f, 1f, t));
            });

            Write(VignettePath, 256, 512, (x, y, w, h) =>
            {
                float dx = (x + 0.5f) / w * 2f - 1f;
                float dy = (y + 0.5f) / h * 2f - 1f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy) / Mathf.Sqrt(2f);
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 1f, distance)) * 0.85f;
                return new Color(0f, 0f, 0f, alpha);
            });

            AssetDatabase.Refresh();
            Configure(FadeEdgePath);
            Configure(VignettePath);
            Debug.Log("BakeUiTextures: baked T_FadeEdge + T_Vignette");
        }

        private static void Write(string path, int width, int height, System.Func<int, int, int, int, Color> pixel)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, pixel(x, y, width, height));
                }
            }

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void Configure(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}
