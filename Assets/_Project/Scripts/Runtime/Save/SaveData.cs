using System;
using System.Collections.Generic;

namespace Spaa.Save
{
    [Serializable]
    public class SaveData
    {
        private const int ElementCount = 5;

        public int[] elementLevels = NewOnesArray();
        public int playerHp = 100;
        public bool takeDamageEnabled = true;
        public string currentDayKey = string.Empty;
        public int monsterSeed;
        public int lastMonsterSeed;
        public int monsterHp;
        public List<string> completedHabitIds = new List<string>();
        public List<CustomHabitData> customHabits = new List<CustomHabitData>();
        public int nextCustomHabitNumber = 1;
        public bool habitsSeeded;
        public bool levelUpPending;
        public string[] lastCompletedDateKeyPerElement = new string[ElementCount];
        public List<DayRecord> history = new List<DayRecord>();

        private static int[] NewOnesArray()
        {
            var levels = new int[ElementCount];
            for (int i = 0; i < ElementCount; i++)
            {
                levels[i] = 1;
            }

            return levels;
        }
    }
}
