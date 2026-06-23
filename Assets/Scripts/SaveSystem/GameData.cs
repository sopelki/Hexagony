using System.Collections.Generic;
using System.Linq;
using Logic.Castle;
using Logic.Tower;
using Logic.Trap;
using UnityEngine;

namespace SaveSystem
{
    public class GameData : MonoBehaviour
    {
        public static GameData Instance;

        public List<TowerData> allTowers;
        public List<TrapData> allTraps;
        public List<BuildingData> allBuildings;

        private void Awake()
        {
            Instance = this;
        }

        public TowerData GetTowerData(TowerType type)
        {
            return allTowers.FirstOrDefault(t => t.type == type);
        }

        public TrapData GetTrapData(TrapType type)
        {
            return allTraps.FirstOrDefault(t => t.trapType == type);
        }

        public BuildingData GetBuildingData(BuildingType type)
        {
            return allBuildings.FirstOrDefault(b => b.type == type);
        }
    }
}