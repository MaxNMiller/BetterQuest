using System;

namespace Spaa.Flow
{
    [Serializable]
    public struct SceneTransition
    {
        public GameState state;
        public string sceneName;
    }
}
