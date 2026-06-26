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
        public GameFlowManager(WaveManager waveManager, TowerSystem towerSystem, TrapSystem trapSystem,
            CastleSystem castleSystem, SlidingNotificationUI hintUI)
        {
            this.waveManager = waveManager;
            this.towerSystem = towerSystem;
            this.trapSystem = trapSystem;
            this.castleSystem = castleSystem;
            this.hintUI = hintUI;
        }

        private readonly CastleSystem castleSystem;

        private bool gameStarted;
        private const float HintCycleInterval = 20f;
        private bool hintCycleStarted;
        private const float HintStartDelay = 5f;
        private readonly SlidingNotificationUI hintUI;
        private const float StartGameDelay = 0.5f;
        private float timeSinceLastHint;
        private float timeSinceObjectPlaced;
        private float timeSinceStart;
        private readonly TowerSystem towerSystem;
        private readonly TrapSystem trapSystem;
        private bool waitingToStart;
        private readonly WaveManager waveManager;

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
            Debug.Log("[GameFlow] Waiting for player to build something...");
        }

        public void ResetToStandardMode()
        {
            IsTutorialActive = false;
            gameStarted = false;
            waitingToStart = false;
            hintCycleStarted = false;
            timeSinceStart = 0f;

            towerSystem.OnFirstTowerPlaced -= OnFirstObjectPlaced;
            trapSystem.OnFirstTrapPlaced -= OnFirstObjectPlaced;
            castleSystem.OnFirstBuildingPlaced -= OnFirstObjectPlaced;

            towerSystem.OnFirstTowerPlaced += OnFirstObjectPlaced;
            trapSystem.OnFirstTrapPlaced += OnFirstObjectPlaced;
            castleSystem.OnFirstBuildingPlaced += OnFirstObjectPlaced;
        }

        private void OnFirstObjectPlaced()
        {
            if (IsTutorialActive || gameStarted || waitingToStart) return;

            waitingToStart = true;
            timeSinceObjectPlaced = 0f;

            if (hintUI) hintUI.HideHint();

            Debug.Log("[GameFlow] First object placed! Starting engine...");
        }

        private void ShowHintWindow()
        {
            if (hintUI) hintUI.ShowHint("Для начала\nзащитите замок!");

            timeSinceLastHint = 0f;
        }

        private void StartGame()
        {
            gameStarted = true;

            TickManager.Instance.OnTick -= Tick;
            towerSystem.OnFirstTowerPlaced -= OnFirstObjectPlaced;
            trapSystem.OnFirstTrapPlaced -= OnFirstObjectPlaced;
            castleSystem.OnFirstBuildingPlaced -= OnFirstObjectPlaced;

            waveManager.StartGame();
        }

        private void Tick()
        {
            if (gameStarted || IsTutorialActive) return;

            if (Time.timeScale > 0)
            {
                var deltaTime = TickManager.Instance.tickInterval / Time.timeScale;

                if (waitingToStart)
                {
                    timeSinceObjectPlaced += deltaTime;
                    if (timeSinceObjectPlaced >= StartGameDelay) StartGame();
                    return;
                }

                timeSinceStart += deltaTime;
                timeSinceLastHint += deltaTime;

                if (!hintCycleStarted)
                {
                    if (timeSinceStart >= HintStartDelay)
                    {
                        hintCycleStarted = true;
                        ShowHintWindow();
                    }
                }
                else
                {
                    if (timeSinceLastHint >= HintCycleInterval) ShowHintWindow();
                }
            }
        }
    }
}