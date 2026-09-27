using System;
using System.Collections.Generic;

namespace Spaa.Tutorial
{
    [Serializable]
    public class TutorialProgress
    {
        public int version;
        public string currentStepId = string.Empty;
        public bool completed;
        public bool skipped;
        public bool isReplay;
        public bool firstHabitWritten;
        public List<string> seenTips = new List<string>();

        public static TutorialProgress FirstRun(string firstStepId, int version)
        {
            return new TutorialProgress { version = version, currentStepId = firstStepId };
        }

        public static TutorialProgress CompletedSilently(int version)
        {
            return new TutorialProgress { version = version, completed = true };
        }
    }
}
