using System.Collections.Generic;
using UnityEngine;
using Logic.Unit;
using Interfaces;
using UnityEngine.Tilemaps;

namespace Logic.Monster
{
    public class MonsterSpawner : ITickable
    {
        public event System.Action OnWaveSpawnCompleted;

        public bool IsLastWave => currentWaveIndex == waves.Count - 1;
        // Может понадобиться потом
        public bool IsWaveFullySpawned => 
            CurrentWave != null && spawnedInCurrentWave >= CurrentWave.totalMonsters;

        private readonly List<Vector2Int> spawnHexes;
        private readonly Field.Field field;
        private readonly MonsterSystem monsterSystem;
        private readonly UnitSystem unitSystem;
        private readonly Tilemap tilemap;
        private readonly List<WaveData> waves;

        private int currentWaveIndex = -1;
        private int spawnedInCurrentWave;
        private float spawnTimer;
        private bool waveSpawnFinished;

        private WaveData CurrentWave =>
            currentWaveIndex >= 0 && currentWaveIndex < waves.Count
                ? waves[currentWaveIndex]
                : null;

        public MonsterSpawner(
            List<Vector2Int> spawnHexes,
            Field.Field field,
            MonsterSystem monsterSystem,
            UnitSystem unitSystem,
            List<WaveData> waves,
            Tilemap tilemap)
        {
            this.spawnHexes = spawnHexes;
            this.field = field;
            this.monsterSystem = monsterSystem;
            this.unitSystem = unitSystem;
            this.waves = waves;
            this.tilemap = tilemap;
        }

        public void StartNextWave()
        {
            currentWaveIndex++;

            if (currentWaveIndex >= waves.Count)
            {
                Debug.Log("MonsterSpawner: No more waves to start.");
                return;
            }

            spawnedInCurrentWave = 0;
            spawnTimer = 0f;
            waveSpawnFinished = false;

            Debug.Log($"MonsterSpawner: Wave {currentWaveIndex + 1} started.");
        }

        public void Tick()
        {
            if (CurrentWave == null || waveSpawnFinished)
                return;

            if (spawnedInCurrentWave >= CurrentWave.totalMonsters)
            {
                waveSpawnFinished = true;
                OnWaveSpawnCompleted?.Invoke();
                return;
            }

            spawnTimer += Core.TickManager.Instance.tickInterval;

            if (spawnTimer >= CurrentWave.spawnInterval)
            {
                spawnTimer = 0f;
                Spawn(CurrentWave);
            }
        }

        private void Spawn(WaveData wave)
        {
            var hex = spawnHexes[Random.Range(0, spawnHexes.Count)];
            var hexObj = field.GetHex(hex);

            if (hexObj == null) return;

            var world = tilemap.GetCellCenterWorld(hexObj.offset);
            var data = wave.monsterPool[Random.Range(0, wave.monsterPool.Count)];

            var monster = new MonsterModel(
                world,
                hex,
                data,
                wave.healthMultiplier,
                wave.damageMultiplier,
                wave.speedMultiplier
            );

            var movement = new HexMoveToTargetStrategy(monster, field, tilemap);
            var attack = new MonsterAttackStrategy(monster, unitSystem);
            monster.SetStrategies(movement, attack);

            monsterSystem.AddMonster(monster);
            spawnedInCurrentWave++;
        }
    }
}