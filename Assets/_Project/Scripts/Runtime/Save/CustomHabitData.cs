using System;

namespace Spaa.Save
{
    [Serializable]
    public class CustomHabitData
    {
        public string id;
        public string displayName;
        public int element;
        public int basePower;
        public int timesPerDay;
        public int inputType;
    }
}
