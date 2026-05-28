using System;
using Core;
using Interfaces;
using UnityEngine;

namespace Logic.Monster
{
    public class WaveManager : ITickable
    {
        private readonly float delayBetweenWaves;
        private readonly MonsterSystem monsterSystem;

        private readonly MonsterSpawner spawner;
        private int currentWaveNumber;
        private float delayTimer;

        private bool gameStarted;
        private bool isDelaying;

        private bool waitingForNextWave;

        public WaveManager(MonsterSpawner spawner, MonsterSystem monsterSystem, float delayBetweenWaves)
        {
            this.spawner = spawner;
            this.monsterSystem = monsterSystem;
            this.delayBetweenWaves = delayBetweenWaves;

            this.spawner.OnWaveSpawnCompleted += OnWaveFinishedSpawning;
        }

        public void Tick()
        {
            if (!gameStarted)
                return;

            if (waitingForNextWave && monsterSystem.GetAllMonsters().Count == 0)
            {
                waitingForNextWave = false;

                if (spawner.IsLastWave)
                {
                    Debug.Log("WaveManager: All enemies cleared on the last wave. Game Won!");
                    OnGameWon?.Invoke();
                    return;
                }

                isDelaying = true;
                delayTimer = delayBetweenWaves;
                Debug.Log($"WaveManager: Wave cleared. Delaying for {delayBetweenWaves}s...");
            }

            if (isDelaying)
            {
                delayTimer -= TickManager.Instance.tickInterval;

                if (delayTimer <= 0f)
                {
                    isDelaying = false;
                    currentWaveNumber++;
                    OnWaveStarting?.Invoke(currentWaveNumber);
                    spawner.StartNextWave();
                }
            }
        }

        public event Action OnGameWon;
        public event Action<int> OnWaveStarting;

        public void StartGame()
        {
            if (gameStarted)
            {
                Debug.LogWarning("Game already started!");
                return;
            }

            gameStarted = true;
            currentWaveNumber = 1;
            OnWaveStarting?.Invoke(currentWaveNumber);
            spawner.StartNextWave();
            Debug.Log("Game started! First wave incoming...");
        }

        // public void StartFirstWave()
        // {
        //     currentWaveNumber = 1;
        //     OnWaveStarting?.Invoke(currentWaveNumber);
        //     spawner.StartNextWave();
        // }

        private void OnWaveFinishedSpawning()
        {
            waitingForNextWave = true;
        }
    }
}