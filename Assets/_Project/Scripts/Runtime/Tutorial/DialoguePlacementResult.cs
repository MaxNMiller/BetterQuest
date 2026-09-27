namespace Spaa.Tutorial
{
    public readonly struct DialoguePlacementResult
    {
        public float Top { get; }
        public TutorialArrow Arrow { get; }
        public bool CoversHole { get; }

        public DialoguePlacementResult(float top, TutorialArrow arrow, bool coversHole = false)
        {
            Top = top;
            Arrow = arrow;
            CoversHole = coversHole;
        }
    }
}
