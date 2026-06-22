using System;
using System.Collections.Generic;
using Logic.Castle;
using Logic.Tower;
using Logic.Trap;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class GameSessionData
    {
        public int currentWaveNumber;
        public int castleHp;
        public int gold;
        
        public List<TowerSaveData> towers = new();
        public List<TrapSaveData> traps = new();
        public List<BuildingSaveData> buildings = new();
    }

    [Serializable]
    public class TowerSaveData
    {
        public TowerType type;
        public int level;
        public Vector3Int gridPosition;
        public Vector3 worldPosition;
    }

    [Serializable]
    public class TrapSaveData
    {
        public TrapType type;
        public Vector2Int centerHex;
    }

    [Serializable]
    public class BuildingSaveData
    {
        public BuildingType type;
    }
}