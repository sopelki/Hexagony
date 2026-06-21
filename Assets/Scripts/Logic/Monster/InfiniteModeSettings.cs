using UnityEngine;

namespace Logic.Monster
{
    [CreateAssetMenu(menuName = "Monsters/Infinite Mode Settings")]
    public class InfiniteModeSettings : ScriptableObject
    {
        [Header("Scaling Rates (Multiplier per wave)")]
        public float healthMultiplierStep = 1.1f;
        public float damageMultiplierStep = 1.05f;
        public float speedMultiplierStep = 1.01f;

        [Header("Spawn Settings")]
        public int monstersCountStep = 2;
        public float spawnIntervalStep = 0.05f;
        public float minSpawnInterval = 0.3f;

        [Header("Base for Infinite Mode")]
        public WaveData referenceWave;
    }
}