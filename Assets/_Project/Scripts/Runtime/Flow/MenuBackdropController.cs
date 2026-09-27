using UnityEngine;

namespace Spaa.Flow
{
    public class MenuBackdropController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Peak yaw sway either side, in degrees.")]
        [SerializeField] private float swayDegrees = 2f;
        [SerializeField] private float swayPeriod = 12f;
        [SerializeField] private Light[] torchLights;
        [Tooltip("Flicker amplitude as a fraction of each light's base intensity.")]
        [SerializeField] private float flickerAmount = 0.08f;
        [SerializeField] private float flickerSpeed = 3f;

        private Quaternion _baseRotation;
        private float[] _baseIntensities;

        private void OnEnable()
        {
            if (cameraTransform != null)
            {
                _baseRotation = cameraTransform.localRotation;
            }

            _baseIntensities = new float[torchLights != null ? torchLights.Length : 0];
            for (int i = 0; i < _baseIntensities.Length; i++)
            {
                _baseIntensities[i] = torchLights[i] != null ? torchLights[i].intensity : 0f;
            }
        }

        private void OnDisable()
        {
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = _baseRotation;
            }

            for (int i = 0; i < _baseIntensities.Length; i++)
            {
                if (torchLights[i] != null)
                {
                    torchLights[i].intensity = _baseIntensities[i];
                }
            }
        }

        private void Update()
        {
            float time = Time.time;
            if (cameraTransform != null && swayPeriod > 0f)
            {
                float yaw = Mathf.Sin(time * (2f * Mathf.PI / swayPeriod)) * swayDegrees;
                cameraTransform.localRotation = _baseRotation * Quaternion.Euler(0f, yaw, 0f);
            }

            for (int i = 0; i < _baseIntensities.Length; i++)
            {
                if (torchLights[i] == null)
                {
                    continue;
                }

                float noise = Mathf.PerlinNoise(time * flickerSpeed, i * 7.31f) * 2f - 1f;
                torchLights[i].intensity = _baseIntensities[i] * (1f + noise * flickerAmount);
            }
        }
    }
}
