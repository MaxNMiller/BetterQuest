using UnityEngine;
using UnityEngine.SceneManagement;
using Spaa.Events;

namespace Spaa.Flow
{
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private GameStateEventChannelSO onRequestGameState;
        [SerializeField] private SceneTransition[] transitions;

        private void OnEnable()
        {
            if (onRequestGameState != null)
            {
                onRequestGameState.RegisterListener(HandleStateRequested);
            }
        }

        private void OnDisable()
        {
            if (onRequestGameState != null)
            {
                onRequestGameState.UnregisterListener(HandleStateRequested);
            }
        }

        private void HandleStateRequested(GameState state)
        {
            if (SceneTransitionLookup.TryFindScene(transitions, state, out string sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
