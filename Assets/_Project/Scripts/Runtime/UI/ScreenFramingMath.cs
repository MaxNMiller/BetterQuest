using UnityEngine;

namespace Spaa.UI
{
    public static class ScreenFramingMath
    {
        public static float BandCentreFromTop(float topCovered, float bottomCovered)
        {
            float top = Mathf.Clamp01(topCovered);
            float bottom = Mathf.Clamp01(bottomCovered);
            if (top + bottom >= 1f)
            {
                return 0.5f;
            }

            return top + (1f - top - bottom) * 0.5f;
        }

        public static float ComposerY(float topCovered, float bottomCovered, float maxOffset)
        {
            float y = BandCentreFromTop(topCovered, bottomCovered) - 0.5f;
            float limit = Mathf.Abs(maxOffset);
            return Mathf.Clamp(y, -limit, limit);
        }
    }
}
