namespace Spaa.Battle
{
    public interface IBattleCommand
    {
        string Label { get; }

        CommandResult Execute(BattleState state);

        void Undo(BattleState state);
    }
}
