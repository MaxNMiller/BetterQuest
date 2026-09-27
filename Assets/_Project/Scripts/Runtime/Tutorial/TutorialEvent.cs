using Spaa.Flow;

namespace Spaa.Tutorial
{
    public readonly struct TutorialEvent
    {
        public TutorialSignal Signal { get; }
        public GameState State { get; }

        public TutorialEvent(TutorialSignal signal, GameState state = default)
        {
            Signal = signal;
            State = state;
        }

        public static TutorialEvent Of(TutorialSignal signal)
        {
            return new TutorialEvent(signal);
        }

        public static TutorialEvent Entered(GameState state)
        {
            return new TutorialEvent(TutorialSignal.StateEntered, state);
        }
    }
}
