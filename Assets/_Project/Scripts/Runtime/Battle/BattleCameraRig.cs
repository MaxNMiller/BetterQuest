using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Spaa.Events;

namespace Spaa.Battle
{
    public class BattleCameraRig : MonoBehaviour
    {
        private const int InactivePriority = 0;
        private const int HitPriority = 20;
        private const int VictoryPriority = 30;

        [SerializeField] private CinemachineCamera idleCamera;
        [SerializeField] private CinemachineOrbitalFollow idleOrbit;
        [SerializeField] private CinemachineCamera hitCamera;
        [SerializeField] private CinemachineCamera victoryCamera;
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [Tooltip("Horizontal sway amplitude of the idle orbit, in degrees.")]
        [SerializeField] private float swayDegrees = 18f;
        [Tooltip("Seconds for one full left-right-left sway.")]
        [SerializeField] private float swayPeriod = 16f;
        [Tooltip("How long the hit cam holds before blending back to idle.")]
        [SerializeField] private float hitHoldSeconds = 0.35f;
        [SerializeField] private AttackResultEventChannelSO onAttackResolved;
        [SerializeField] private VoidEventChannelSO onMonsterDefeated;

        private Coroutine _hitRoutine;
        private float _swayTime;
        private bool _victory;

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

        private void Start()
        {
            SetPriority(hitCamera, InactivePriority);
            SetPriority(victoryCamera, InactivePriority);
        }

        private void Update()
        {
            if (idleOrbit == null || swayPeriod <= 0f)
            {
                return;
            }

            _swayTime += Time.deltaTime;
            idleOrbit.HorizontalAxis.Value = Mathf.Sin(_swayTime * (2f * Mathf.PI / swayPeriod)) * swayDegrees;
        }

        private void HandleAttackResolved(CommandResult result)
        {
            if (!result.Success || _victory)
            {
                return;
            }

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithForce(CameraImpulseMath.ForceFor(result));
            }

            if (_hitRoutine != null)
            {
                StopCoroutine(_hitRoutine);
            }

            _hitRoutine = StartCoroutine(HoldHitCamera());
        }

        private void HandleMonsterDefeated()
        {
            _victory = true;
            if (_hitRoutine != null)
            {
                StopCoroutine(_hitRoutine);
                _hitRoutine = null;
            }

            SetPriority(hitCamera, InactivePriority);
            SetPriority(victoryCamera, VictoryPriority);

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithForce(CameraImpulseMath.DefeatForce);
            }
        }

        private IEnumerator HoldHitCamera()
        {
            SetPriority(hitCamera, HitPriority);
            yield return new WaitForSeconds(hitHoldSeconds);
            SetPriority(hitCamera, InactivePriority);
            _hitRoutine = null;
        }

        private static void SetPriority(CinemachineCamera cam, int priority)
        {
            if (cam != null)
            {
                cam.Priority = priority;
            }
        }
    }
}
