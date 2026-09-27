using UnityEngine;
using Spaa.Elements;

namespace Spaa.Battle
{
    public class ArenaBuilder : MonoBehaviour
    {
        private static readonly int RingColorId = Shader.PropertyToID("_RingColor");
        private static readonly int TopColorId = Shader.PropertyToID("_TopColor");
        private static readonly int HorizonColorId = Shader.PropertyToID("_HorizonColor");
        private static readonly int BottomColorId = Shader.PropertyToID("_BottomColor");

        [SerializeField] private Renderer groundRenderer;
        [SerializeField] private Light keyLight;
        [SerializeField] private Transform propsRoot;
        [Tooltip("Authored scene props enabled only for the current monster's element.")]
        [SerializeField] private ElementPropGroup[] authoredProps = System.Array.Empty<ElementPropGroup>();

        [System.Serializable]
        private struct ElementPropGroup
        {
            public Element element;
            public GameObject root;
        }

        [Tooltip("Spaa/SkyGradient material; instanced and tinted per element.")]
        [SerializeField] private Material skyMaterial;
        [Tooltip("Spaa/Toon material for spawned props; null keeps the primitive's default material.")]
        [SerializeField] private Material propMaterial;
        [Tooltip("Top-of-sky color the element tint is blended toward.")]
        [SerializeField] private Color skyZenithColor = new Color(0.22f, 0.32f, 0.72f);

        private Material _skyInstance;

        private void OnDestroy()
        {
            if (_skyInstance != null)
            {
                Destroy(_skyInstance);
            }
        }

        public void Build(ElementDefinition definition)
        {
            ClearProps();

            foreach (var group in authoredProps)
            {
                if (group.root != null)
                {
                    group.root.SetActive(group.element == definition.Element);
                }
            }

            if (groundRenderer != null)
            {
                var groundMaterial = groundRenderer.material;
                groundMaterial.color = definition.ArenaGroundColor;
                groundMaterial.SetColor(RingColorId, Color.Lerp(definition.Color, Color.white, 0.25f));
            }

            if (keyLight != null)
            {
                keyLight.color = definition.ArenaLightColor;
            }

            RenderSettings.fog = true;
            RenderSettings.fogColor = definition.ArenaFogColor;
            ApplySky(definition);

            if (propsRoot == null)
            {
                return;
            }

            var props = ArenaLayout.GetProps(definition.Element);
            foreach (var spec in props)
            {
                var prop = GameObject.CreatePrimitive(spec.Primitive);
                Destroy(prop.GetComponent<Collider>());
                var propTransform = prop.transform;
                propTransform.SetParent(propsRoot, false);
                propTransform.localPosition = spec.LocalPosition;
                propTransform.localScale = spec.LocalScale;
                propTransform.localEulerAngles = spec.LocalEulerAngles;
                var propRenderer = prop.GetComponent<Renderer>();
                if (propMaterial != null)
                {
                    propRenderer.sharedMaterial = propMaterial;
                }

                propRenderer.material.color = definition.Color;
            }
        }

        private void ApplySky(ElementDefinition definition)
        {
            if (skyMaterial == null)
            {
                return;
            }

            if (_skyInstance == null)
            {
                _skyInstance = new Material(skyMaterial);
            }

            _skyInstance.SetColor(TopColorId, Color.Lerp(definition.Color, skyZenithColor, 0.55f));
            _skyInstance.SetColor(HorizonColorId, definition.ArenaFogColor);
            _skyInstance.SetColor(BottomColorId, Color.Lerp(definition.ArenaFogColor, definition.ArenaGroundColor, 0.5f));
            RenderSettings.skybox = _skyInstance;
            DynamicGI.UpdateEnvironment();
        }

        private void ClearProps()
        {
            if (propsRoot == null)
            {
                return;
            }

            for (int i = propsRoot.childCount - 1; i >= 0; i--)
            {
                var child = propsRoot.GetChild(i).gameObject;
                var childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    Destroy(childRenderer.material);
                }

                Destroy(child);
            }
        }
    }
}
