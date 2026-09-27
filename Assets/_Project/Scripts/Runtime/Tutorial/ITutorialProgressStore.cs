namespace Spaa.Tutorial
{
    public interface ITutorialProgressStore
    {
        TutorialProgress Load();

        void Save(TutorialProgress progress);
    }
}
