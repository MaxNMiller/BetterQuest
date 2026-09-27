using System;
using System.Collections.Generic;
using Spaa.Battle;
using Spaa.Elements;

namespace Spaa.Progression
{
    public class ElementLevels : IElementLevelProvider
    {
        private readonly Dictionary<Element, int> levels = new Dictionary<Element, int>();

        public ElementLevels()
        {
            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                levels[element] = 1;
            }
        }

        public int GetLevel(Element element)
        {
            return levels[element];
        }

        public void SetLevel(Element element, int level)
        {
            levels[element] = level;
        }

        public void IncrementLevel(Element element)
        {
            levels[element]++;
        }
    }
}
