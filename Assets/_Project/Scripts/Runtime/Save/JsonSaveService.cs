using System;
using System.IO;
using UnityEngine;

namespace Spaa.Save
{
    public class JsonSaveService : ISaveService
    {
        private readonly string filePath;

        public JsonSaveService(string filePath)
        {
            this.filePath = filePath;
        }

        public SaveData Load()
        {
            if (!File.Exists(filePath))
            {
                return new SaveData();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                return data ?? new SaveData();
            }
            catch (Exception)
            {
                return new SaveData();
            }
        }

        public void Save(SaveData data)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, json);
        }
    }
}
