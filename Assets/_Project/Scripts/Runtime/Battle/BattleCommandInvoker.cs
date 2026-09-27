using System.Collections.Generic;

namespace Spaa.Battle
{
    public class BattleCommandInvoker
    {
        private readonly BattleState state;
        private readonly List<IBattleCommand> history = new List<IBattleCommand>();

        public BattleCommandInvoker(BattleState state)
        {
            this.state = state;
        }

        public CommandResult ExecuteCommand(IBattleCommand command)
        {
            var result = command.Execute(state);
            if (result.Success)
            {
                history.Add(command);
            }

            return result;
        }

        public bool UndoLast()
        {
            if (history.Count == 0)
            {
                return false;
            }

            var last = history[history.Count - 1];
            history.RemoveAt(history.Count - 1);
            last.Undo(state);
            return true;
        }
    }
}
