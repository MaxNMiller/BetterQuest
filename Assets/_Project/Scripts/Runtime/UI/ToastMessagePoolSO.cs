using UnityEngine;

namespace Spaa.UI
{
    [CreateAssetMenu(menuName = "Spaa/UI/Toast Message Pool", fileName = "New Toast Message Pool")]
    public class ToastMessagePoolSO : ScriptableObject
    {
        [SerializeField]
        private string[] neutralMessages =
        {
            "Great job!",
            "Nice work!",
            "Keep it up!",
            "Hydration hero!",
        };

        [SerializeField]
        private string[] weaknessMessages =
        {
            "Super effective!",
            "Critical care!",
            "Perfect match!",
        };

        public string[] NeutralMessages => neutralMessages;
        public string[] WeaknessMessages => weaknessMessages;
    }
}
