using System;
using System.Collections.Generic;
using Audio;
using Core;
using Interfaces;
using Logic.Trap;
using Logic.Unit;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Logic.Monster
{
    public class MonsterSpawner : ITickable
    {
        public MonsterSpawner(List<Vector2Int> spawnHexes, Field.Field field, MonsterSystem monsterSystem,
            UnitSystem unitSystem, List<WaveData> waves, Tilemap tilemap, TrapSystem trapSystem, SoundData soundData)
        {
            this.spawnHexes = spawnHexes;
            this.field = field;
            this.monsterSystem = monsterSystem;
            this.unitSystem = unitSystem;
            this.waves = waves;
            this.tilemap = tilemap;
            this.trapSystem = trapSystem;
            this.soundData = soundData;
        }

        private int currentWaveIndex = -1;
        private readonly Field.Field field;

        private InfiniteModeSettings infiniteSettings;
        private bool isInfiniteMode;
        private readonly MonsterSystem monsterSystem;
        private readonly SoundData soundData;
        private int spawnedInCurrentWave;

        private readonly List<Vector2Int> spawnHexes;
        private float spawnTimer;
        private readonly Tilemap tilemap;
        private readonly TrapSystem trapSystem;
        private readonly UnitSystem unitSystem;
        private readonly List<WaveData> waves;
        private bool waveSpawnFinished;

        public bool IsLastWave => !isInfiniteMode && currentWaveIndex == waves.Count - 1;

        public int NormalWaves => waves.Count;

        public event Action OnWaveSpawnCompleted;

        public void EnableInfiniteMode(InfiniteModeSettings settings)
        {
            infiniteSettings = settings;
            isInfiniteMode = true;
        }

        public void SetWaveIndex(int index)
        {
            currentWaveIndex = index;
        }

        public void StartNextWave()
        {
            currentWaveIndex++;

            if (!isInfiniteMode && currentWaveIndex >= waves.Count)
            {
                Debug.Log("MonsterSpawner: All manual waves completed.");
                return;
            }

            spawnedInCurrentWave = 0;
            spawnTimer = 0f;
            waveSpawnFinished = false;

            Debug.Log($"MonsterSpawner: Wave {currentWaveIndex + 1} started. (Infinite: {isInfiniteMode})");
        }

        public void Tick()
        {
            if (currentWaveIndex < 0) return;

            int totalMonsters;
            float spawnInterval;

            if (currentWaveIndex < waves.Count)
            {
                totalMonsters = waves[currentWaveIndex].totalMonsters;
                spawnInterval = waves[currentWaveIndex].spawnInterval;
            }
            else if (isInfiniteMode)
            {
                var extraWaves = currentWaveIndex - waves.Count + 1;
                totalMonsters = infiniteSettings.referenceWave.totalMonsters +
                                extraWaves * infiniteSettings.monstersCountStep;
                spawnInterval = Mathf.Max(infiniteSettings.minSpawnInterval,
                    infiniteSettings.referenceWave.spawnInterval - extraWaves * infiniteSettings.spawnIntervalStep);
            }
            else return;

            if (waveSpawnFinished) return;

            if (spawnedInCurrentWave >= totalMonsters)
            {
                waveSpawnFinished = true;
                OnWaveSpawnCompleted?.Invoke();
                return;
            }

            spawnTimer += TickManager.Instance.tickInterval;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                Spawn(totalMonsters);
            }
        }

        private void Spawn(int totalMonsters)
        {
            var hex = spawnHexes[Random.Range(0, spawnHexes.Count)];
            var hexObj = field.GetHex(hex);
            if (hexObj == null) return;

            var world = tilemap.GetCellCenterWorld(hexObj.offset);

            MonsterData data;
            float hMult, dMult, sMult;

            var diffStatMult = DifficultyManager.GetStatMultiplier();
            var diffGoldMult = DifficultyManager.GetGoldMultiplier();
            
            var isLastSpawn = (spawnedInCurrentWave == totalMonsters - 1);

            if (currentWaveIndex < NormalWaves)
            {
                var wave = waves[currentWaveIndex];
                if (isLastSpawn && wave.bossMonster != null)
                    data = wave.bossMonster;
                else
                    data = wave.monsterPool[Random.Range(0, wave.monsterPool.Count)];
                hMult = wave.healthMultiplier;
                dMult = wave.damageMultiplier;
                sMult = wave.speedMultiplier;
            }
            else
            {
                var extraWaves = currentWaveIndex - NormalWaves + 1;
                var wave = infiniteSettings.referenceWave;
                if (isLastSpawn && wave.bossMonster != null)
                    data = wave.bossMonster;
                else
                    data = wave.monsterPool[Random.Range(0, wave.monsterPool.Count)];

                hMult = wave.healthMultiplier * Mathf.Pow(infiniteSettings.healthMultiplierStep, extraWaves);
                dMult = wave.damageMultiplier * Mathf.Pow(infiniteSettings.damageMultiplierStep, extraWaves);
                sMult = wave.speedMultiplier * Mathf.Pow(infiniteSettings.speedMultiplierStep, extraWaves);
            }

            hMult *= diffStatMult;
            dMult *= diffStatMult;
            sMult *= diffStatMult;

            var monster = new MonsterModel(world, hex, data, hMult, dMult, sMult, diffGoldMult, soundData);
            var movement = new HexMoveToTargetStrategy(monster, field, tilemap, trapSystem, monsterSystem);
            var attack = new MonsterAttackStrategy(monster, unitSystem, soundData);
            monster.SetStrategies(movement, attack);
            monsterSystem.AddMonster(monster);
            spawnedInCurrentWave++;
        }
    }
}