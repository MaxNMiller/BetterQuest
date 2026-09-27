using Spaa.Elements;

namespace Spaa.Battle
{
    public readonly struct CommandResult
    {
        public bool Success { get; }
        public int DamageDealt { get; }
        public bool WasWeakness { get; }
        public bool MonsterDefeated { get; }
        public string Message { get; }
        public Element Element { get; }

        public CommandResult(bool success, int damageDealt, bool wasWeakness, bool monsterDefeated, string message)
            : this(success, damageDealt, wasWeakness, monsterDefeated, message, default)
        {
        }

        public CommandResult(bool success, int damageDealt, bool wasWeakness, bool monsterDefeated, string message, Element element)
        {
            Success = success;
            DamageDealt = damageDealt;
            WasWeakness = wasWeakness;
            MonsterDefeated = monsterDefeated;
            Message = message;
            Element = element;
        }

        public static CommandResult Failed(string message)
        {
            return new CommandResult(false, 0, false, false, message);
        }
    }
}
