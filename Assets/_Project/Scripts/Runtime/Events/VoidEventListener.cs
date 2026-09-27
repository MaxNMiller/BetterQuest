using UnityEngine;
using UnityEngine.Events;

namespace Spaa.Events
{
    public class VoidEventListener : MonoBehaviour
    {
        [Tooltip("The channel to listen to.")]
        [SerializeField] private VoidEventChannelSO channel;
        [Tooltip("Invoked when the channel is raised.")]
        [SerializeField] private UnityEvent response;

        private void OnEnable()
        {
            channel.RegisterListener(OnEventRaised);
        }

        private void OnDisable()
        {
            channel.UnregisterListener(OnEventRaised);
        }

        private void OnEventRaised()
        {
            response.Invoke();
        }
    }
}
