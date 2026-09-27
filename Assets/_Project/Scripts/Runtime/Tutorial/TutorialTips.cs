using System.Collections.Generic;
using Spaa.Flow;

namespace Spaa.Tutorial
{
    public static class TutorialTips
    {
        public static TutorialTipData Pick(IReadOnlyList<TutorialTipData> tips, TutorialProgress progress,
            GameState entered, TutorialScene scene, bool hasBattled)
        {
            if (tips == null || progress == null || !progress.completed)
            {
                return null;
            }

            for (int i = 0; i < tips.Count; i++)
            {
                var tip = tips[i];
                if (tip == null || string.IsNullOrEmpty(tip.Id) || tip.Callout == null)
                {
                    continue;
                }

                if (tip.Trigger != entered || tip.Scene != scene || (tip.AfterFirstBattle && !hasBattled))
                {
                    continue;
                }

                if (progress.seenTips != null && progress.seenTips.Contains(tip.Id))
                {
                    continue;
                }

                return tip;
            }

            return null;
        }

        public static bool MarkSeen(TutorialProgress progress, string tipId)
        {
            if (progress == null || string.IsNullOrEmpty(tipId))
            {
                return false;
            }

            if (progress.seenTips == null)
            {
                progress.seenTips = new List<string>();
            }

            if (progress.seenTips.Contains(tipId))
            {
                return false;
            }

            progress.seenTips.Add(tipId);
            return true;
        }
    }
}
