using System.Collections.Generic;
using Spaa.Elements;

namespace Spaa.Flow
{
    public class TodayStatus
    {
        public TodayStatusKind Kind { get; }
        public Element? Weakness { get; }
        public string MonsterName { get; }
        public IReadOnlyList<Element> MissingElements { get; }

        public TodayStatus(TodayStatusKind kind, Element? weakness, string monsterName, IReadOnlyList<Element> missingElements)
        {
            Kind = kind;
            Weakness = weakness;
            MonsterName = monsterName;
            MissingElements = missingElements ?? new List<Element>();
        }
    }
}
