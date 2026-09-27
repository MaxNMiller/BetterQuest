namespace Spaa.Battle
{
    public class MonsterEndOfDayAttackCommand : IBattleCommand
    {
        private readonly int damageAmount;
        private int previousPlayerHp;

        public string Label => "Monster End-of-Day Attack";

        public MonsterEndOfDayAttackCommand(int damageAmount)
        {
            this.damageAmount = damageAmount;
        }

        public CommandResult Execute(BattleState state)
        {
            previousPlayerHp = state.PlayerHp;

            int applied = state.TakeDamageEnabled ? state.ApplyDamageToPlayer(damageAmount) : 0;

            return new CommandResult(true, applied, false, false, "Your monster attacks!");
        }

        public void Undo(BattleState state)
        {
            state.SetPlayerHp(previousPlayerHp);
        }
    }
}
