using System.IO;
using UnityEngine;

namespace Spaa.Save
{
    public static class SaveLocation
    {
        public const string FileName = "save.json";

        public static string DefaultFilePath => Path.Combine(Application.persistentDataPath, FileName);

        public const string TutorialFileName = "tutorial.json";

        public static string TutorialFilePath => Path.Combine(Application.persistentDataPath, TutorialFileName);
    }
}
