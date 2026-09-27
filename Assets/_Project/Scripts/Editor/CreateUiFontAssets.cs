using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Spaa.Editor
{
    public static class CreateUiFontAssets
    {
        private const string OutputFolder = "Assets/_Project/Fonts/FontAssets";

        [MenuItem("Tools/Spaa/UI/Repair Font Assets")]
        public static void Repair()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode before repairing font assets.");
                return;
            }

            Create();
            string characters = string.Empty;
            for (int code = 32; code <= 126; code++)
                characters += (char)code;

            foreach (var fontPath in SourceFonts)
            {
                string target = $"{OutputFolder}/{Path.GetFileNameWithoutExtension(fontPath)} SDF.asset";
                var asset = AssetDatabase.LoadAssetAtPath<FontAsset>(target);
                var source = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
                if (asset == null || source == null || asset.sourceFontFile != source)
                {
                    Debug.LogError($"Repair Font Assets: missing asset or mismatched source for {target}");
                    continue;
                }

                asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                asset.ClearFontAssetData();
                bool added = asset.TryAddCharacters(characters, out string missing);
                if (!added || !string.IsNullOrEmpty(missing) || asset.atlasTexture.width <= 1)
                {
                    Debug.LogError($"Repair Font Assets: glyph generation failed for {target}; missing: {missing}");
                    continue;
                }

                asset.material.mainTexture = asset.atlasTexture;
                foreach (var subasset in AssetDatabase.LoadAllAssetsAtPath(target))
                    EditorUtility.SetDirty(subasset);
                AssetDatabase.SaveAssetIfDirty(asset);
                Debug.Log($"Repair Font Assets: rebuilt {asset.name} (95 ASCII characters).");
            }
        }

        private static readonly string[] SourceFonts =
        {
            "Assets/_Project/Fonts/Fredoka/static/Fredoka-SemiBold.ttf",
            "Assets/_Project/Fonts/Fredoka/static/Fredoka-Bold.ttf",
            "Assets/_Project/Fonts/Nunito/static/Nunito-Regular.ttf",
            "Assets/_Project/Fonts/Nunito/static/Nunito-Bold.ttf",
            "Assets/_Project/Fonts/Nunito/static/Nunito-ExtraBold.ttf",
        };

        [MenuItem("Tools/Spaa/UI/Create Font Assets")]
        public static void Create()
        {
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Fonts", "FontAssets");
            }

            foreach (var fontPath in SourceFonts)
            {
                string name = Path.GetFileNameWithoutExtension(fontPath) + " SDF";
                string target = $"{OutputFolder}/{name}.asset";
                if (AssetDatabase.LoadAssetAtPath<FontAsset>(target) != null)
                {
                    continue;
                }

                var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
                if (font == null)
                {
                    Debug.LogError($"CreateUiFontAssets: missing {fontPath}");
                    continue;
                }

                Selection.activeObject = font;
                EditorApplication.ExecuteMenuItem("Assets/Create/UI Toolkit/Text/Font Asset/SDF");

                string created = $"{Path.GetDirectoryName(fontPath).Replace('\\', '/')}/{name}.asset";
                string error = AssetDatabase.MoveAsset(created, target);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"CreateUiFontAssets: move {created} -> {target} failed: {error}");
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<FontAsset>(target);
                Debug.Log($"CreateUiFontAssets: {target} mode={asset.atlasPopulationMode}");
            }

            AssetDatabase.SaveAssets();
        }
    }
}
