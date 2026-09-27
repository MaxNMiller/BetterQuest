using System;
using UnityEngine;

namespace Spaa.Events
{
    public abstract class EventChannelSO<T> : ScriptableObject
    {
        private event Action<T> onEventRaised;

        public void Raise(T value)
        {
            onEventRaised?.Invoke(value);
        }

        public void RegisterListener(Action<T> listener)
        {
            onEventRaised += listener;
        }

        public void UnregisterListener(Action<T> listener)
        {
            onEventRaised -= listener;
        }
    }
}
