using System.IO;
using UnityEngine;

namespace Field
{
    public class SaveLoadManager
    {
        private static readonly string saveFilePath = Path
            .Combine(Application.dataPath, "Scripts", "Levels", "MyLevel.json")
            .Replace(Path.DirectorySeparatorChar, '/');

        public static void SaveMapToFile(Field fieldToSave)
        {
            var data = fieldToSave.ExportToSaveData();
            var json = JsonUtility.ToJson(data, true);

            File.WriteAllText(saveFilePath, json);

            Debug.Log($"Field is saved\nPath: {saveFilePath}");
        }

        public static Field LoadMapFromFile()
        {
            if (File.Exists(saveFilePath))
            {
                var json = File.ReadAllText(saveFilePath);
                var data = JsonUtility.FromJson<FieldData>(json);

                var loadedField = new Field();
                loadedField.ImportFromFieldData(data);

                Debug.Log("File is loaded.");
                return loadedField;
            }
            Debug.LogError("File not found.");
            return null;
        }
    }
}