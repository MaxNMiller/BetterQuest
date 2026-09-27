using System;
using Spaa.Elements;
using Spaa.Save;

namespace Spaa.Progression
{
    public class LevelUpService
    {
        public void ApplyUpgrade(ElementLevels levels, Element element)
        {
            levels.IncrementLevel(element);
        }

        public void SaveTo(SaveData data, ElementLevels levels)
        {
            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                data.elementLevels[(int)element] = levels.GetLevel(element);
            }
        }

        public ElementLevels LoadFrom(SaveData data)
        {
            var levels = new ElementLevels();
            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                levels.SetLevel(element, data.elementLevels[(int)element]);
            }

            return levels;
        }
    }
}
