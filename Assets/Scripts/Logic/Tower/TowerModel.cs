using UnityEngine;

namespace Logic.Tower
{
    public class TowerModel
    {
        public TowerModel(TowerData data, Vector3Int gridPos, Vector3 worldPos)
        {
            Data = data;
            GridPosition = gridPos;
            WorldPosition = worldPos;
        }

        public TowerData Data { get; }
        public Vector3Int GridPosition { get; }
        public Vector3 WorldPosition { get; }
        public float CooldownTimer { get; set; }
        public int ShotsLeft { get; set; }
    }
}