using UnityEngine;

namespace Core
{
    public class DifficultyManager
    {
        private const string DifficultyKey = "GameDifficulty";

        public static GameDifficulty CurrentDifficulty
        {
            get => (GameDifficulty)PlayerPrefs.GetInt(DifficultyKey, (int)GameDifficulty.Normal);
            set
            {
                PlayerPrefs.SetInt(DifficultyKey, (int)value);
                PlayerPrefs.Save();
            }
        }
        
        public static (float healthMult, float dmgMult, float goldMult) GetCurrentMultipliers()
        {
            return CurrentDifficulty switch
            {
                GameDifficulty.Easy   => (0.75f, 0.75f, 1.33f),
                GameDifficulty.Hard   => (1.25f, 1.25f, 0.8f),
                _                     => (1.0f, 1.0f, 1.0f)
            };
        }

        public static float GetStatMultiplier()
        {
            return CurrentDifficulty switch
            {
                GameDifficulty.Easy => 0.75f,
                GameDifficulty.Hard => 1.25f,
                _ => 1f
            };
        }

        public static float GetGoldMultiplier()
        {
            return CurrentDifficulty switch
            {
                GameDifficulty.Easy => 1.33f, 
                GameDifficulty.Hard => 0.8f,  
                _ => 1f
            };
        }
    }
}