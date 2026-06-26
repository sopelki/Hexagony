using System;
using Core;
using Interfaces;
using UnityEngine;

namespace Logic.Monster
{
    public class WaveManager : ITickable
    {
        public WaveManager(MonsterSpawner spawner, MonsterSystem monsterSystem, float delayBetweenWaves)
        {
            this.spawner = spawner;
            this.monsterSystem = monsterSystem;
            this.delayBetweenWaves = delayBetweenWaves;

            this.spawner.OnWaveSpawnCompleted += OnWaveFinishedSpawning;
        }

        private readonly float delayBetweenWaves;

        private float delayTimer;

        private bool gameStarted;
        private const string HighScoreKey = "MaxWaveReached";
        private bool isDelaying;
        private readonly MonsterSystem monsterSystem;

        private readonly MonsterSpawner spawner;

        private bool waitingForNextWave;

        public int CurrentWaveNumber { get; private set; }

        public event Action OnGameWon;
        public event Action OnInfiniteModeStarted;

        public event Action<int> OnWaveCleared;
        public event Action<int> OnWaveStarting;

        public void StartGame()
        {
            if (gameStarted)
            {
                Debug.LogWarning("Game already started!");
                return;
            }

            gameStarted = true;
            CurrentWaveNumber = 1;
            SaveHighScore(CurrentWaveNumber);
            OnWaveStarting?.Invoke(CurrentWaveNumber);
            spawner.StartNextWave();
            Debug.Log("Game started! First wave incoming...");
        }

        public void StartInfiniteMode(InfiniteModeSettings settings)
        {
            spawner.EnableInfiniteMode(settings);
            waitingForNextWave = false;
            isDelaying = true;
            delayTimer = delayBetweenWaves;
            OnInfiniteModeStarted?.Invoke();
        }

        public void StartSavedGame(int savedWaveNumber, InfiniteModeSettings infiniteSettings)
        {
            if (gameStarted) return;

            gameStarted = true;
            CurrentWaveNumber = savedWaveNumber;
            var maxNormalWaves = spawner.NormalWaves;

            if (savedWaveNumber >= maxNormalWaves)
                spawner.EnableInfiniteMode(infiniteSettings);
            spawner.SetWaveIndex(savedWaveNumber - 1);

            waitingForNextWave = false;
            isDelaying = true;
            delayTimer = delayBetweenWaves;
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
                OnWaveCleared?.Invoke(CurrentWaveNumber);
                Debug.Log($"WaveManager: Wave cleared. Delaying for {delayBetweenWaves}s...");
            }

            if (isDelaying)
            {
                delayTimer -= TickManager.Instance.tickInterval;

                if (delayTimer <= 0f)
                {
                    isDelaying = false;
                    CurrentWaveNumber++;
                    SaveHighScore(CurrentWaveNumber);
                    OnWaveStarting?.Invoke(CurrentWaveNumber);
                    spawner.StartNextWave();
                }
            }
        }

        private void OnWaveFinishedSpawning()
        {
            waitingForNextWave = true;
        }

        private static void SaveHighScore(int wave)
        {
            var currentBest = PlayerPrefs.GetInt(HighScoreKey, 0);
            if (wave > currentBest)
            {
                PlayerPrefs.SetInt(HighScoreKey, wave);
                PlayerPrefs.Save();
            }
        }
    }
}