using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Spaa.Tutorial
{
    public class TutorialProgressStore : ITutorialProgressStore
    {
        private readonly string _filePath;

        public TutorialProgressStore(string filePath)
        {
            _filePath = filePath;
        }

        public bool LastLoadWasCorrupt { get; private set; }

        public TutorialProgress Load()
        {
            LastLoadWasCorrupt = false;
            if (!File.Exists(_filePath))
            {
                return null;
            }

            TutorialProgress progress = null;
            try
            {
                string json = File.ReadAllText(_filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    progress = JsonUtility.FromJson<TutorialProgress>(json);
                }
            }
            catch (Exception)
            {
                progress = null;
            }

            if (progress == null)
            {
                LastLoadWasCorrupt = true;
                return TutorialProgress.CompletedSilently(0);
            }

            if (progress.seenTips == null)
            {
                progress.seenTips = new List<string>();
            }

            if (progress.currentStepId == null)
            {
                progress.currentStepId = string.Empty;
            }

            return progress;
        }

        public void Save(TutorialProgress progress)
        {
            if (progress == null)
            {
                return;
            }

            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_filePath, JsonUtility.ToJson(progress));
        }
    }
}
