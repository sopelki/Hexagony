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

        private bool firstBuildingPlaced;
        private float spawnTimer;
        private float currentSpawnInterval = float.PositiveInfinity;

        public static CastleSystem Instance { get; private set; }
        public CastleModel CastleModel { get; }
        public int CurrentUnitsCount => unitSystem?.GetAllUnits().Count ?? 0;

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
        }

        public void Tick()
        {
            var dt = TickManager.Instance.tickInterval;
            spawnTimer += dt;

            if (spawnTimer >= currentSpawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnSingleUnit();
            }
        }

        private void RecalculateSpawnInterval()
        {
            var barracksCount = CastleModel.Buildings.Count(b => b.Data.type == BuildingType.Barracks);

            if (barracksCount == 0)
            {
                currentSpawnInterval = 999f;
                Debug.Log("[SpawnInterval] No barracks, spawn disabled");
                return;
            }

            currentSpawnInterval = unitData.baseSpawnInterval * Mathf.Pow(0.8f, barracksCount - 1);

            Debug.Log($"[SpawnInterval] Barracks: {barracksCount}, Interval: {currentSpawnInterval:F2}s");
        }

        private void HandleUnitDied(UnitModel unit)
        {
            CastleModel.Changed();
        }

        public void RegisterCastleData(List<Vector3> worldPositions, List<Vector2Int> hexes)
        {
            CastleModel.WallWorldPositions = worldPositions;
            CastleModel.WallHexes = hexes;
        }

        public bool CanAfford(int price) => CastleModel.Gold >= price;

        public bool TrySpendGold(int price)
        {
            if (TutorialManager.IsTutorialActive()) return true;
            if (CastleModel.Gold < price) return false;

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
            if (!TrySpendGold(data.baseCost)) return false;

            var instance = new BuildingModel(data);
            CastleModel.Buildings.Add(instance);

            if (data.type == BuildingType.Farm)
                CastleModel.MaxSupply += data.supplyProvided;

            ApplyBuff(data);

            if (soundData?.buildingPlaceSound != null)
                AudioManager.Instance.PlaySfx(soundData.buildingPlaceSound, soundData.buildingPlacementVolume);

            if (!firstBuildingPlaced)
            {
                firstBuildingPlaced = true;
                OnFirstBuildingPlaced?.Invoke();
            }

            if (data.type == BuildingType.Barracks)
                RecalculateSpawnInterval();

            CastleModel.Changed();
            return true;
        }

        private void TrySpawnSingleUnit()
        {
            var barracksCount = CastleModel.Buildings.Count(b => b.Data.type == BuildingType.Barracks);
            if (barracksCount == 0) return;

            if (unitSystem.GetAllUnits().Count >= CastleModel.MaxSupply) return;

            SpawnUnit();
        }

        private void SpawnUnit()
        {
            var hex = field.GetHex(spawnHex);
            if (hex == null) return;

            var worldPos = tilemap.GetCellCenterWorld(hex.offset);
            unitSystem.SpawnUnit(worldPos, spawnHex, unitData);
            CastleModel.Changed();
        }

        private void ApplyBuff(BuildingData data)
        {
            switch (data.type)
            {
                case BuildingType.Blacksmith:
                    unitSystem.AddBuff(new AttackPercentBuff(data.buffValue));
                    break;
                case BuildingType.Hospital:
                    unitSystem.AddBuff(new HealthPercentBuff(data.buffValue));
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

            currentSpawnInterval = 999f;

            CastleModel.Changed();
        }
    }
}