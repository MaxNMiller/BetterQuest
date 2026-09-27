using UnityEngine;

namespace Spaa.UI
{
    public static class SafeAreaMath
    {
        public static SafeAreaMargins ComputeMargins(Rect safeArea, float screenWidth, float screenHeight)
        {
            float left = safeArea.xMin;
            float right = screenWidth - safeArea.xMax;
            float top = screenHeight - safeArea.yMax;
            float bottom = safeArea.yMin;

            return new SafeAreaMargins(
                Mathf.Max(0f, left),
                Mathf.Max(0f, right),
                Mathf.Max(0f, top),
                Mathf.Max(0f, bottom));
        }
    }
}
