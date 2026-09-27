using UnityEngine;
using Spaa.Elements;

namespace Spaa.Battle
{
    public class MonsterFaceView : MonoBehaviour
    {
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private MonsterFaceLibrary library;
        [SerializeField] private Renderer[] eyeRenderers;
        [SerializeField] private Renderer[] mouthRenderers;

        private Material[] _eyeMaterials = System.Array.Empty<Material>();
        private Material[] _mouthMaterials = System.Array.Empty<Material>();

        public MonsterFaceLibrary Library => library;

        private void Awake()
        {
            _eyeMaterials = InstanceMaterials(eyeRenderers);
            _mouthMaterials = InstanceMaterials(mouthRenderers);
            if (library != null)
            {
                SetColor(_eyeMaterials, library.FaceColor);
                SetColor(_mouthMaterials, library.FaceColor);
            }
        }

        public void Show(MonsterExpression expression, Element element)
        {
            if (library == null)
            {
                return;
            }

            var face = library.GetFace(expression, element);
            SetTexture(_eyeMaterials, face.Eye);
            SetTexture(_mouthMaterials, face.Mouth);
        }

        private static Material[] InstanceMaterials(Renderer[] renderers)
        {
            if (renderers == null)
            {
                return System.Array.Empty<Material>();
            }

            var materials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                materials[i] = renderers[i] != null ? renderers[i].material : null;
            }

            return materials;
        }

        private static void SetTexture(Material[] materials, Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            foreach (var material in materials)
            {
                if (material != null)
                {
                    material.SetTexture(BaseMapId, texture);
                }
            }
        }

        private static void SetColor(Material[] materials, Color color)
        {
            foreach (var material in materials)
            {
                if (material != null)
                {
                    material.SetColor(BaseColorId, color);
                }
            }
        }
    }
}
