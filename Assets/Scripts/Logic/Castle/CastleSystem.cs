using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Interfaces;
using Logic.Unit;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Logic.Castle
{
    public class CastleSystem : ITickable
    {
        public event Action OnFirstBuildingPlaced;
        public static CastleSystem Instance { get; private set; }

        private static readonly Vector2Int spawnHex = new(-28, 21); // Поменять, если нужна другая точка спавна
        private readonly UnitSystem unitSystem;
        private readonly UnitData unitData;
        private readonly Field.Field field;
        private readonly Tilemap tilemap;
        private float resourceTimer;
        private float spawnTimer;
        private const float ResourceInterval = 1f;
        private const float SpawnInterval = 1f;
        private readonly SoundData soundData;

        private bool firstBuildingPlaced;

        public CastleSystem(
            CastleModel model,
            UnitSystem unitSystem,
            UnitData unitData,
            Field.Field field,
            Tilemap tilemap,
            SoundData soundData)
        {
            Model = model;
            this.unitSystem = unitSystem;
            this.unitData = unitData;
            this.field = field;
            this.tilemap = tilemap;
            this.soundData = soundData;
            Instance = this;
        }

        public CastleModel Model { get; }

        public void RegisterCastleData(List<Vector3> worldPositions, List<Vector2Int> hexes)

        {
            Model.WallWorldPositions = worldPositions;
            Model.WallHexes = hexes;
            Debug.Log($"Castle registered in logic. Wall hexes count: {hexes.Count}");
        }

        public void Tick()
        {
            var dt = Core.TickManager.Instance.tickInterval;

            resourceTimer += dt;
            spawnTimer += dt;

            if (resourceTimer >= ResourceInterval)
            {
                resourceTimer = 0f;
                ProduceResources();
            }

            if (!(spawnTimer >= SpawnInterval))
                return;
            spawnTimer = 0f;
            SpawnUnitsFromBarracks();
        }
        
        public bool CanAfford(int price)
        {
            return Model.Gold >= price;
        }
        
        public bool TrySpendGold(int price)
        {
            if (Model.Gold < price)
                return false;

            Model.Gold -= price;
            Model.Changed();
            return true;
        }

        public void AddGold(int amount)
        {
            Model.Gold += amount;
            Model.Changed();
        }


        public bool TryBuyBuilding(BuildingData data)
        {
            if (!TrySpendGold(data.baseCost))
                return false;

            var instance = new BuildingModel(data);
            Model.Buildings.Add(instance);

            ApplyBuff(data);
            
            if (soundData != null && soundData.buildingPlaceSound != null)
                AudioManager.Instance.PlaySfx(soundData.buildingPlaceSound, soundData.buildingPlacementVolume);

            if (!firstBuildingPlaced)
            {
                firstBuildingPlaced = true;
                OnFirstBuildingPlaced?.Invoke();
                Debug.Log("First building placed. Game can start.");
            }

            return true;
        }

        private void SpawnUnitsFromBarracks()
        {
            var barracksCount = Model.Buildings.Count(b => b.Data.type == BuildingType.Barracks);

            if (barracksCount == 0)
                return;

            for (var i = 0; i < barracksCount; i++)
            {
                if (Model.Food < unitData.foodCost)
                    return;

                SpawnUnit();
            }
        }

        // TODO: Поменять логику использования еды юнитами (например, чтобы они тратили еду, находясь на поле)
        private void SpawnUnit()
        {
            var hex = field.GetHex(spawnHex);

            if (hex == null)
                return;

            var worldPos = tilemap.GetCellCenterWorld(hex.offset);
            unitSystem.SpawnUnit(worldPos, spawnHex, unitData);
            Model.Food -= unitData.foodCost;
            Model.CurrentUnits++;
            Model.Changed();
        }


        private void ProduceResources()
        {
            var changed = false;
            foreach (var building in Model.Buildings.Where(building => building.Data.type == BuildingType.Farm))
            {
                Model.Food += building.Production;
                changed = true;
            }

            if (changed)
                Model.Changed();
        }

        private void ApplyBuff(BuildingData data)
        {
            switch (data.type)
            {
                case BuildingType.Blacksmith:
                    unitSystem.AddBuff(new AttackPercentBuff(data.buffValue));
                    Debug.Log($"Blacksmith built: units get +{data.buffValue * 100}% attack.");
                    break;
                case BuildingType.Hospital:
                    unitSystem.AddBuff(new HealthPercentBuff(data.buffValue));
                    Debug.Log($"Hospital built: units get +{data.buffValue * 100}% health.");
                    break;
                case BuildingType.Farm:
                case BuildingType.Barracks:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}