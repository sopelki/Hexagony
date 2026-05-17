using Interfaces;
using UnityEngine;

namespace Logic.Monster
{
    public class WaveManager : ITickable
    {
        public event System.Action OnGameWon;
        public event System.Action<int> OnWaveStarting;

        private readonly MonsterSpawner spawner;
        private readonly MonsterSystem monsterSystem;

        private bool waitingForNextWave;
        private bool isDelaying;
        private float delayTimer;
        private readonly float delayBetweenWaves;
        private int currentWaveNumber;

        public WaveManager(MonsterSpawner spawner, MonsterSystem monsterSystem, float delayBetweenWaves)
        {
            this.spawner = spawner;
            this.monsterSystem = monsterSystem;
            this.delayBetweenWaves = delayBetweenWaves;

            this.spawner.OnWaveSpawnCompleted += OnWaveFinishedSpawning;
        }

        public void Tick()
        {
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
                delayTimer -= Core.TickManager.Instance.tickInterval;

                if (delayTimer <= 0f)
                {
                    isDelaying = false;
                    currentWaveNumber++;
                    OnWaveStarting?.Invoke(currentWaveNumber);
                    spawner.StartNextWave();
                }
            }
        }

        private void OnWaveFinishedSpawning() => waitingForNextWave = true;

        public void StartFirstWave()
        {
            currentWaveNumber = 1;
            OnWaveStarting?.Invoke(currentWaveNumber);
            spawner.StartNextWave();
        }
    }
}