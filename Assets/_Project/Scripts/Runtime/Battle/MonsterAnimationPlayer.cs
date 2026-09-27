using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Spaa.Battle
{
    [RequireComponent(typeof(Animator))]
    public class MonsterAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip takeDamageClip;
        [SerializeField] private AnimationClip dealDamageClip;
        [SerializeField] private AnimationClip deathClip;
        [Tooltip("Seconds to blend one-shots in from idle and back out to the continuing idle animation.")]
        [Min(0f)]
        [SerializeField] private float crossfade = 0.12f;

        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private AnimationClipPlayable _oneShot;
        private float _oneShotLength;
        private bool _oneShotHolds;

        public bool IsHoldingFinalPose => _oneShot.IsValid() && _oneShotHolds;

        private void OnEnable()
        {
            _graph = PlayableGraph.Create($"{name}.MonsterAnimation");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "Animation", GetComponent<Animator>());
            _mixer = AnimationMixerPlayable.Create(_graph, 2);
            output.SetSourcePlayable(_mixer);

            if (idleClip != null)
            {
                var idle = AnimationClipPlayable.Create(_graph, idleClip);
                _graph.Connect(idle, 0, _mixer, 0);
            }

            _mixer.SetInputWeight(0, 1f);
            _graph.Play();
        }

        private void OnDisable()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        private void Update()
        {
            if (!_oneShot.IsValid())
            {
                return;
            }

            float time = (float)_oneShot.GetTime();
            if (MonsterAnimationBlend.IsFinished(time, _oneShotLength, _oneShotHolds))
            {
                ClearOneShot();
                return;
            }

            float weight = MonsterAnimationBlend.OneShotWeight(time, _oneShotLength, crossfade, _oneShotHolds);
            _mixer.SetInputWeight(0, 1f - weight);
            _mixer.SetInputWeight(1, weight);
        }

        public float Play(MonsterAnimation animation)
        {
            if (!_graph.IsValid() || IsHoldingFinalPose)
            {
                return 0f;
            }

            if (animation == MonsterAnimation.Idle)
            {
                ClearOneShot();
                return 0f;
            }

            var clip = ClipFor(animation);
            if (clip == null)
            {
                return 0f;
            }

            ClearOneShot();
            _oneShot = AnimationClipPlayable.Create(_graph, clip);
            _oneShot.SetTime(0);
            _graph.Connect(_oneShot, 0, _mixer, 1);
            _oneShotLength = clip.length;
            _oneShotHolds = animation == MonsterAnimation.Death;
            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);
            return clip.length;
        }

        private AnimationClip ClipFor(MonsterAnimation animation)
        {
            switch (animation)
            {
                case MonsterAnimation.TakeDamage:
                    return takeDamageClip;
                case MonsterAnimation.DealDamage:
                    return dealDamageClip;
                case MonsterAnimation.Death:
                    return deathClip;
                default:
                    return idleClip;
            }
        }

        private void ClearOneShot()
        {
            if (_oneShot.IsValid())
            {
                _graph.Disconnect(_mixer, 1);
                _oneShot.Destroy();
            }

            _oneShot = default;
            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);
        }
    }
}
