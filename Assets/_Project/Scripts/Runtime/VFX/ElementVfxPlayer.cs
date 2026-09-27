using System.Collections.Generic;
using UnityEngine;
using Spaa.Battle;
using Spaa.Elements;
using Spaa.Events;

namespace Spaa.VFX
{
    public class ElementVfxPlayer : MonoBehaviour
    {
        private static readonly int ShapeId = Shader.PropertyToID("_Shape");

        [Tooltip("Material using Spaa/Particle; instanced per shape at runtime.")]
        [SerializeField] private Material particleMaterial;
        [SerializeField] private ElementDefinition[] elementDefinitions;
        [Tooltip("Where hit effects spawn (the monster's body).")]
        [SerializeField] private Transform effectAnchor;
        [SerializeField] private Vector3 anchorOffset = new Vector3(0f, 0.2f, -0.8f);
        [Tooltip("Multiplies particle size, speed, spread and gravity so effects match the monster's scale.")]
        [SerializeField] private float effectScale = 1f;
        [SerializeField] private AttackResultEventChannelSO onAttackResolved;
        [SerializeField] private VoidEventChannelSO onMonsterDefeated;

        private readonly ParticleSystem[] _elementSystems = new ParticleSystem[5];
        private readonly Dictionary<int, Material> _shapeMaterials = new Dictionary<int, Material>();
        private ParticleSystem _hitBurst;
        private ParticleSystem _shockwave;

        private void Awake()
        {
            for (int i = 0; i < _elementSystems.Length; i++)
            {
                var element = (Element)i;
                var profile = ElementVfxProfiles.For(element);
                _elementSystems[i] = CreateSystem($"Vfx_{element}", profile, ColorFor(element));
            }

            _hitBurst = CreateSystem("Vfx_HitBurst", ElementVfxProfiles.HitBurst, Color.white);
            _shockwave = CreateShockwave();

            foreach (var system in GetComponentsInChildren<ParticleSystem>())
            {
                system.Play();
            }
        }

        private void OnEnable()
        {
            if (onAttackResolved != null)
            {
                onAttackResolved.RegisterListener(HandleAttackResolved);
            }

            if (onMonsterDefeated != null)
            {
                onMonsterDefeated.RegisterListener(HandleMonsterDefeated);
            }
        }

        private void OnDisable()
        {
            if (onAttackResolved != null)
            {
                onAttackResolved.UnregisterListener(HandleAttackResolved);
            }

            if (onMonsterDefeated != null)
            {
                onMonsterDefeated.UnregisterListener(HandleMonsterDefeated);
            }
        }

        private void OnDestroy()
        {
            foreach (var material in _shapeMaterials.Values)
            {
                Destroy(material);
            }

            _shapeMaterials.Clear();
        }

        public void Play(Element element, bool isWeakness)
        {
            Vector3 position = AnchorPosition();
            var profile = ElementVfxProfiles.For(element);
            var system = _elementSystems[(int)element];
            system.transform.position = position;
            system.Emit(ElementVfxProfiles.EmitCount(profile, isWeakness));

            _hitBurst.transform.position = position;
            _hitBurst.Emit(ElementVfxProfiles.EmitCount(ElementVfxProfiles.HitBurst, isWeakness));

            if (profile.Shockwave || isWeakness)
            {
                EmitShockwave(ColorFor(element));
            }
        }

        private void HandleAttackResolved(CommandResult result)
        {
            if (!result.Success)
            {
                return;
            }

            Play(result.Element, result.WasWeakness);
        }

        private void HandleMonsterDefeated()
        {
            Vector3 position = AnchorPosition();
            for (int i = 0; i < _elementSystems.Length; i++)
            {
                _elementSystems[i].transform.position = position;
                _elementSystems[i].Emit(ElementVfxProfiles.For((Element)i).BurstCount);
            }

            _hitBurst.transform.position = position;
            _hitBurst.Emit(ElementVfxProfiles.MaxParticlesPerEffect);
            EmitShockwave(Color.white);
        }

        private Vector3 AnchorPosition()
        {
            return effectAnchor != null ? effectAnchor.position + anchorOffset : transform.position;
        }

        private void EmitShockwave(Color color)
        {
            var emitParams = new ParticleSystem.EmitParams
            {
                position = new Vector3(AnchorPosition().x, 0.05f, AnchorPosition().z),
                applyShapeToPosition = false,
                startColor = color,
            };
            _shockwave.Emit(emitParams, 1);
        }

        private Color ColorFor(Element element)
        {
            if (elementDefinitions != null)
            {
                foreach (var definition in elementDefinitions)
                {
                    if (definition != null && definition.Element == element)
                    {
                        return definition.Color;
                    }
                }
            }

            return Color.white;
        }

        private Material MaterialFor(int shape)
        {
            if (particleMaterial == null)
            {
                return null;
            }

            if (!_shapeMaterials.TryGetValue(shape, out var material))
            {
                material = new Material(particleMaterial);
                material.SetFloat(ShapeId, shape);
                _shapeMaterials[shape] = material;
            }

            return material;
        }

        private ParticleSystem CreateSystem(string systemName, ElementVfxProfile profile, Color color)
        {
            var system = CreateBareSystem(systemName, ElementVfxProfiles.MaxParticlesPerEffect * 2);

            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(profile.Lifetime * 0.7f, profile.Lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(profile.Speed * 0.5f * effectScale, profile.Speed * effectScale);
            main.startSize = new ParticleSystem.MinMaxCurve(profile.Size * 0.6f * effectScale, profile.Size * effectScale);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.5f));
            main.gravityModifier = profile.GravityModifier * effectScale;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, profile.Shape == ElementVfxProfile.ShapeStar ? Mathf.PI : 0f);

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = Mathf.Max(0.01f, profile.EmitRadius * effectScale);

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.4f), new Keyframe(0.15f, 1f), new Keyframe(1f, 0.6f)));

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = MaterialFor(profile.Shape);
            if (profile.Stretched)
            {
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.velocityScale = 0.08f;
                renderer.lengthScale = 2.5f;
            }

            return system;
        }

        private ParticleSystem CreateShockwave()
        {
            var system = CreateBareSystem("Vfx_Shockwave", 4);

            var main = system.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = 7f * effectScale;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1f));

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = MaterialFor(ElementVfxProfile.ShapeRing);
            renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;

            return system;
        }

        private ParticleSystem CreateBareSystem(string systemName, int maxParticles)
        {
            var go = new GameObject(systemName);
            go.transform.SetParent(transform, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = system.emission;
            emission.enabled = false;

            var shape = system.shape;
            shape.enabled = false;

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return system;
        }
    }
}
