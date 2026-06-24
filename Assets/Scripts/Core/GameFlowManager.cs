using Logic.Castle;
using Logic.Monster;
using Logic.Tower;
using Logic.Trap;
using UI;
using UnityEngine;

namespace Core
{
    public class GameFlowManager
    {
        private const float HintStartDelay = 2f;
        private const float HintCycleInterval = 4f;
        private const float StartGameDelay = 0.5f;
        private readonly CastleSystem castleSystem;
        private readonly SlidingNotificationUI hintUI;
        private readonly TowerSystem towerSystem;
        private readonly TrapSystem trapSystem;
        private readonly WaveManager waveManager;

        private bool gameStarted;
        private bool hintCycleStarted;
        private float timeSinceLastHint;
        private float timeSinceObjectPlaced;
        private float timeSinceStart;
        private bool waitingToStart;

        public GameFlowManager(
            WaveManager waveManager,
            TowerSystem towerSystem,
            TrapSystem trapSystem,
            CastleSystem castleSystem,
            SlidingNotificationUI hintUI)
        {
            this.waveManager = waveManager;
            this.towerSystem = towerSystem;
            this.trapSystem = trapSystem;
            this.castleSystem = castleSystem;
            this.hintUI = hintUI;
        }

        public bool IsTutorialActive { get; set; } = true;

        public void Initialize()
        {
            towerSystem.OnFirstTowerPlaced += OnFirstObjectPlaced;
            trapSystem.OnFirstTrapPlaced += OnFirstObjectPlaced;
            castleSystem.OnFirstBuildingPlaced += OnFirstObjectPlaced;

            timeSinceStart = 0f;
            timeSinceLastHint = 0f;
            timeSinceObjectPlaced = 0f;
            hintCycleStarted = false;
            gameStarted = false;
            waitingToStart = false;

            TickManager.Instance.OnTick += Tick;

            Debug.Log("GameFlowManager: Waiting for player action...");
        }

        private void Tick()
        {
            if (gameStarted || IsTutorialActive)
                return;

            if (Time.timeScale > 0)
            {
                var deltaTime = Time.unscaledDeltaTime;

                timeSinceStart += deltaTime;
                timeSinceLastHint += deltaTime;

                if (waitingToStart)
                {
                    timeSinceObjectPlaced += deltaTime;

                    if (timeSinceObjectPlaced >= StartGameDelay)
                    {
                        Debug.Log("GameFlowManager: Game started.");
                        StartGame();
                    }

                    return;
                }

                switch (hintCycleStarted)
                {
                    case false when timeSinceStart >= HintStartDelay:
                        hintCycleStarted = true;
                        ShowHintWindow();
                        break;
                    case true when timeSinceLastHint >= HintCycleInterval:
                        ShowHintWindow();
                        break;
                }

                void ShowHintWindow()
                {
                    if (hintUI)
                        hintUI.ShowHint("Сначала\nзащитите замок!");
                    timeSinceLastHint = hintUI != null ? -hintUI.DisplayDuration : 0f;
                }
            }
        }

        private void OnFirstObjectPlaced()
        {
            if (IsTutorialActive)
                return;

            if (gameStarted || waitingToStart)
                return;

            waitingToStart = true;
            timeSinceObjectPlaced = 0f;

            if (hintUI != null)
                hintUI.HideHint();

            Debug.Log($"First object placed. Starting game in {StartGameDelay}s...");
        }

        private void StartGame()
        {
            gameStarted = true;

            TickManager.Instance.OnTick -= Tick;

            towerSystem.OnFirstTowerPlaced -= OnFirstObjectPlaced;
            trapSystem.OnFirstTrapPlaced -= OnFirstObjectPlaced;
            castleSystem.OnFirstBuildingPlaced -= OnFirstObjectPlaced;

            waveManager.StartGame();

            Debug.Log("GameFlowManager: Game started.");
        }

        public void ResetToStandardMode()
        {
            IsTutorialActive = false;

            towerSystem.OnFirstTowerPlaced -= OnFirstObjectPlaced;
            trapSystem.OnFirstTrapPlaced -= OnFirstObjectPlaced;
            castleSystem.OnFirstBuildingPlaced -= OnFirstObjectPlaced;

            towerSystem.OnFirstTowerPlaced += OnFirstObjectPlaced;
            trapSystem.OnFirstTrapPlaced += OnFirstObjectPlaced;
            castleSystem.OnFirstBuildingPlaced += OnFirstObjectPlaced;

            gameStarted = false;
            waitingToStart = false;
            hintCycleStarted = false;
            timeSinceStart = 0f;

            Debug.Log("GameFlowManager: Сброшен в режим чистой игры.");
        }
    }
}