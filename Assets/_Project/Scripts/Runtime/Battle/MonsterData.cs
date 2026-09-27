using Spaa.Elements;

namespace Spaa.Battle
{
    public class MonsterData
    {
        public string Name { get; }
        public Element Weakness { get; }
        public int MaxHp { get; }
        public int Seed { get; }

        public MonsterData(string name, Element weakness, int maxHp, int seed)
        {
            Name = name;
            Weakness = weakness;
            MaxHp = maxHp;
            Seed = seed;
        }
    }
}
