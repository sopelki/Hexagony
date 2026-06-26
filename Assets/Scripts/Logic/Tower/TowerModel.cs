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

        public float CooldownTimer { get; set; }
        public float CurrentDamage => Data.projectileData.damage * Mathf.Pow(Data.damageMultiplierPerLevel, Level - 1);
        public float CurrentFireRate => Data.fireRate * Mathf.Pow(Data.fireRateMultiplierPerLevel, Level - 1);

        public float CurrentRange => Data.range * Mathf.Pow(Data.rangeMultiplierPerLevel, Level - 1);
        public TowerData Data { get; }
        public Vector3Int GridPosition { get; }
        public int Level { get; private set; }
        public int ShotsLeft { get; set; }
        public Vector3 WorldPosition { get; }

        public event Action<int> OnLevelUp;

        public void Upgrade()
        {
            Level++;
            OnLevelUp?.Invoke(Level);
        }
    }
}