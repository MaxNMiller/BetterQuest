using UnityEngine;

namespace Spaa.Tutorial
{
    public static class DialoguePlacement
    {
        public static DialoguePlacementResult Resolve(Rect hole, bool hasHole, Rect safeArea, float dialogueHeight,
            float gap, TutorialDock dock)
        {
            float topDock = safeArea.yMin + gap;
            float bottomDock = safeArea.yMax - gap - dialogueHeight;

            if (!hasHole)
            {
                switch (dock)
                {
                    case TutorialDock.Top:
                        return new DialoguePlacementResult(topDock, TutorialArrow.None);
                    case TutorialDock.Bottom:
                        return new DialoguePlacementResult(bottomDock, TutorialArrow.None);
                    default:
                        return new DialoguePlacementResult(safeArea.center.y - dialogueHeight * 0.5f, TutorialArrow.None);
                }
            }

            if (dock == TutorialDock.Top && topDock + dialogueHeight + gap <= hole.yMin)
            {
                return new DialoguePlacementResult(topDock, TutorialArrow.Down);
            }

            if (dock == TutorialDock.Bottom && bottomDock - gap >= hole.yMax)
            {
                return new DialoguePlacementResult(bottomDock, TutorialArrow.Up);
            }

            float aboveTop = hole.yMin - gap - dialogueHeight;
            float belowTop = hole.yMax + gap;
            bool fitsAbove = aboveTop >= topDock;
            bool fitsBelow = belowTop <= bottomDock;
            bool preferAbove = hole.center.y > safeArea.center.y;

            if (preferAbove && fitsAbove)
            {
                return new DialoguePlacementResult(aboveTop, TutorialArrow.Down);
            }

            if (fitsBelow)
            {
                return new DialoguePlacementResult(belowTop, TutorialArrow.Up);
            }

            if (fitsAbove)
            {
                return new DialoguePlacementResult(aboveTop, TutorialArrow.Down);
            }

            float roomAbove = hole.yMin - safeArea.yMin;
            float roomBelow = safeArea.yMax - hole.yMax;
            return new DialoguePlacementResult(roomAbove >= roomBelow ? topDock : bottomDock, TutorialArrow.None, true);
        }

        public static float ArrowX(float targetCenterX, float dialogueLeft, float dialogueWidth, float arrowSize, float cornerInset)
        {
            float min = cornerInset;
            float max = Mathf.Max(min, dialogueWidth - cornerInset - arrowSize);
            return Mathf.Clamp(targetCenterX - dialogueLeft - arrowSize * 0.5f, min, max);
        }
    }
}
