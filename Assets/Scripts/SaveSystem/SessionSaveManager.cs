using System.IO;
using UnityEngine;

namespace SaveSystem
{
    public static class SessionSaveManager
    {
        public static bool IsSaveLoaded { get; set; }

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "CurrentSession.json");

        public static void DeleteSession()
        {
            if (File.Exists(SaveFilePath))
                File.Delete(SaveFilePath);
            IsSaveLoaded = false;
        }

        public static bool HasSavedSession()
        {
            return File.Exists(SaveFilePath);
        }

        public static GameSessionData LoadSession()
        {
            if (!File.Exists(SaveFilePath))
                return null;

            var json = File.ReadAllText(SaveFilePath);
            return JsonUtility.FromJson<GameSessionData>(json);
        }

        public static void SaveSession(GameSessionData data)
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[SaveSystem] Игра сохранена: {SaveFilePath}");
        }
    }
}