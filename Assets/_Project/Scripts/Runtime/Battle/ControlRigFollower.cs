using UnityEngine;

namespace Spaa.Battle
{
    public class ControlRigFollower : MonoBehaviour
    {
        [Tooltip("Animated control, e.g. controlRig/.../Main_Ctrl.")]
        [SerializeField] private Transform source;
        [Tooltip("Group holding the skinned joints, e.g. controlRig/SingleHierarchy_Grp.")]
        [SerializeField] private Transform target;

        private Matrix4x4 _sourceRestInverse;
        private Matrix4x4 _targetRest;
        private bool _ready;

        private void Awake()
        {
            if (source == null || target == null || target.parent == null)
            {
                return;
            }

            _sourceRestInverse = SourceInTargetParentSpace().inverse;
            _targetRest = Matrix4x4.TRS(target.localPosition, target.localRotation, target.localScale);
            _ready = true;
        }

        private void LateUpdate()
        {
            if (!_ready)
            {
                return;
            }

            var pose = ComputeFollowPose(SourceInTargetParentSpace(), _sourceRestInverse, _targetRest);
            target.localPosition = pose.GetColumn(3);
            target.localRotation = pose.rotation;
            target.localScale = pose.lossyScale;
        }

        private Matrix4x4 SourceInTargetParentSpace()
        {
            return target.parent.worldToLocalMatrix * source.localToWorldMatrix;
        }

        public static Matrix4x4 ComputeFollowPose(Matrix4x4 sourceNow, Matrix4x4 sourceRestInverse, Matrix4x4 targetRest)
        {
            return sourceNow * sourceRestInverse * targetRest;
        }
    }
}
