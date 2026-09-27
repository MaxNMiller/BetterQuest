using UnityEngine;

namespace Spaa.Tutorial
{
    public static class SpotlightMath
    {
        public const int RectCount = 4;

        public static bool IsUsable(Rect rect)
        {
            return !float.IsNaN(rect.x) && !float.IsNaN(rect.y) && !float.IsNaN(rect.width) && !float.IsNaN(rect.height)
                && rect.width > 0f && rect.height > 0f;
        }

        public static Rect PaddedHole(Rect target, Vector2 panelSize, float padding)
        {
            if (!IsUsable(target))
            {
                return Rect.zero;
            }

            float xMin = Mathf.Clamp(target.xMin - padding, 0f, panelSize.x);
            float yMin = Mathf.Clamp(target.yMin - padding, 0f, panelSize.y);
            float xMax = Mathf.Clamp(target.xMax + padding, 0f, panelSize.x);
            float yMax = Mathf.Clamp(target.yMax + padding, 0f, panelSize.y);
            return Rect.MinMaxRect(xMin, yMin, Mathf.Max(xMin, xMax), Mathf.Max(yMin, yMax));
        }

        public static bool Compute(Rect target, Vector2 panelSize, float padding, Rect[] dims)
        {
            var hole = PaddedHole(target, panelSize, padding);
            if (!IsUsable(hole))
            {
                dims[0] = new Rect(0f, 0f, panelSize.x, panelSize.y);
                dims[1] = Rect.zero;
                dims[2] = Rect.zero;
                dims[3] = Rect.zero;
                return false;
            }

            dims[0] = new Rect(0f, 0f, panelSize.x, hole.yMin);
            dims[1] = new Rect(0f, hole.yMax, panelSize.x, panelSize.y - hole.yMax);
            dims[2] = new Rect(0f, hole.yMin, hole.xMin, hole.height);
            dims[3] = new Rect(hole.xMax, hole.yMin, panelSize.x - hole.xMax, hole.height);
            return true;
        }
    }
}
