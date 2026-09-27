using System.Collections.Generic;

namespace Spaa.Flow
{
    public static class SceneTransitionLookup
    {
        public static bool TryFindScene(IReadOnlyList<SceneTransition> transitions, GameState state, out string sceneName)
        {
            if (transitions != null)
            {
                for (int i = 0; i < transitions.Count; i++)
                {
                    if (transitions[i].state == state)
                    {
                        sceneName = transitions[i].sceneName;
                        return true;
                    }
                }
            }

            sceneName = null;
            return false;
        }
    }
}
