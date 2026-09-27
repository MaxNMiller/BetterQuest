using Spaa.Battle;
using Spaa.Flow;

namespace Spaa.Audio
{
    public static class AudioCueRules
    {
        public static SfxCue ForState(GameState state)
        {
            switch (state)
            {
                case GameState.LevelUp:
                    return SfxCue.LevelUp;
                case GameState.SoftReminder:
                case GameState.Dashboard:
                    return SfxCue.Popup;
                default:
                    return SfxCue.None;
            }
        }

        public static SfxCue ForAttack(CommandResult result)
        {
            return result.Success ? SfxCue.Attack : SfxCue.None;
        }
    }
}
