using System;
using UnityEngine;

namespace Spaa.Events
{
    [CreateAssetMenu(menuName = "Spaa/Events/Void Event Channel", fileName = "New Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        private event Action onEventRaised;

        public void Raise()
        {
            onEventRaised?.Invoke();
        }

        public void RegisterListener(Action listener)
        {
            onEventRaised += listener;
        }

        public void UnregisterListener(Action listener)
        {
            onEventRaised -= listener;
        }
    }
}
