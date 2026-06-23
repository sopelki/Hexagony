using System;
using UnityEngine;

namespace Logic.Tower
{
    public class TowerModel
    {
        public TowerModel(TowerData data, Vector3Int gridPos, Vector3 worldPos, int level = 1)
        {
            Data = data;
            GridPosition = gridPos;
            WorldPosition = worldPos;
            Level = level;
        }

        public TowerData Data { get; }
        public Vector3Int GridPosition { get; }
        public Vector3 WorldPosition { get; }
        public int Level { get; private set; }
        public float CooldownTimer { get; set; }
        public int ShotsLeft { get; set; }

        public float CurrentRange => Data.range * Mathf.Pow(Data.rangeMultiplierPerLevel, Level - 1);
        public float CurrentDamage => Data.projectileData.damage * Mathf.Pow(Data.damageMultiplierPerLevel, Level - 1);
        public float CurrentFireRate => Data.fireRate * Mathf.Pow(Data.fireRateMultiplierPerLevel, Level - 1);

        public event Action<int> OnLevelUp;

        public void Upgrade()
        {
            Level++;
            OnLevelUp?.Invoke(Level);
        }
    }
}