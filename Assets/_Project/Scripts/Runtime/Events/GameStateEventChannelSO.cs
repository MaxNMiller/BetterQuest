using UnityEngine;
using Spaa.Flow;

namespace Spaa.Events
{
    [CreateAssetMenu(menuName = "Spaa/Events/Game State Event Channel", fileName = "New Game State Event Channel")]
    public class GameStateEventChannelSO : EventChannelSO<GameState>
    {
    }
}
