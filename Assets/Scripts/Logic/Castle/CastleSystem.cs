using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Core;
using Interfaces;
using Logic.Unit;
using Misc;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Logic.Castle
{
    public class CastleSystem : ITickable
    {
        private static readonly Vector2Int spawnHex = new(-19, 14);
        private readonly Field.Field field;
        private readonly SoundData soundData;
        private readonly Tilemap tilemap;
        private readonly UnitData unitData;
        private readonly UnitSystem unitSystem;
        private float currentSpawnInterval;
        private bool firstBuildingPlaced;
        private float spawnTimer;
        public readonly CastleModel CastleModel;
        public static CastleSystem Instance { get; private set; }

        public event Action OnFirstBuildingPlaced;

        public CastleSystem(
            CastleModel castleModel,
            UnitSystem unitSystem,
            UnitData unitData,
            Field.Field field,
            Tilemap tilemap,
            SoundData soundData)
        {
            CastleModel = castleModel;
            this.unitSystem = unitSystem;
            this.unitData = unitData;
            this.field = field;
            this.tilemap = tilemap;
            this.soundData = soundData;
            Instance = this;
            this.unitSystem.OnUnitDied += HandleUnitDied;
            CastleModel.OnChanged += RecalculateSpawnSpeed;

            RecalculateSpawnSpeed();
        }

        public int CurrentUnitsCount => unitSystem?.GetAllUnits().Count ?? 0;


        public void Tick()
        {
            if (currentSpawnInterval >= 1000f) return;

            spawnTimer += TickManager.Instance.tickInterval;

            if (spawnTimer >= currentSpawnInterval)
            {
                spawnTimer = 0;
                TrySpawnUnit();
            }
        }

        private void RecalculateSpawnSpeed()
        {
            var barracksCount = CastleModel.Buildings.Count(building => building.Data.type == BuildingType.Barracks);

            if (barracksCount <= 0)
            {
                currentSpawnInterval = float.MaxValue;
                return;
            }

            currentSpawnInterval = unitData.baseSpawnInterval * Mathf.Pow(0.8f, barracksCount - 1);

            Debug.Log($"UnitSystem: Barracks count: {barracksCount}, current interval: {currentSpawnInterval}s");
        }

        private void HandleUnitDied(UnitModel unit)
        {
            CastleModel.Changed();
        }

        public void RegisterCastleData(List<Vector3> worldPositions, List<Vector2Int> hexes)

        {
            CastleModel.WallWorldPositions = worldPositions;
            CastleModel.WallHexes = hexes;
            Debug.Log($"Castle registered in logic. Wall hexes count: {hexes.Count}");
        }

        public bool CanAfford(int price)
        {
            return CastleModel.Gold >= price;
        }

        public bool TrySpendGold(int price)
        {
            if (TutorialManager.IsTutorialActive())
                return true;

            if (CastleModel.Gold < price)
                return false;

            CastleModel.Gold -= price;
            CastleModel.Changed();
            return true;
        }

        public void AddGold(int amount)
        {
            CastleModel.Gold += amount;
            CastleModel.Changed();
        }


        public bool TryBuyBuilding(BuildingData data)
        {
            if (!TrySpendGold(data.baseCost))
                return false;

            var instance = new BuildingModel(data);
            CastleModel.Buildings.Add(instance);

            if (data.type == BuildingType.Farm)
                CastleModel.MaxSupply += data.supplyProvided;

            ApplyBuff(data);

            if (soundData != null && soundData.buildingPlaceSound != null)
                AudioManager.Instance.PlaySfx(soundData.buildingPlaceSound, soundData.buildingPlacementVolume);

            if (!firstBuildingPlaced)
            {
                firstBuildingPlaced = true;
                OnFirstBuildingPlaced?.Invoke();
                Debug.Log("First building placed. Game can start.");
            }

            CastleModel.Changed();
            return true;
        }

        private void TrySpawnUnit()
        {
            if (unitSystem.GetAllUnits().Count >= CastleModel.MaxSupply)
                return;

            SpawnUnit();
        }

        private void SpawnUnit()
        {
            var hex = field.GetHex(spawnHex);

            if (hex == null)
                return;

            var worldPos = tilemap.GetCellCenterWorld(hex.offset);
            unitSystem.SpawnUnit(worldPos, spawnHex, unitData);
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

        public void Clear()
        {
            CastleModel.Buildings.Clear();
            firstBuildingPlaced = false;
            unitSystem?.ClearBuffs();

            CastleModel.OnChanged -= RecalculateSpawnSpeed;
            if (unitSystem != null)
                unitSystem.OnUnitDied -= HandleUnitDied;

            CastleModel.Changed();
        }
    }
}