using System.Collections.Generic;
using Audio;
using Field;
using Logic.Castle;
using Logic.Monster;
using Logic.Projectile;
using Logic.Tower;
using Logic.Trap;
using Logic.Unit;
using MenuScripts;
using Misc;
using SaveSystem;
using UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using View;

namespace Core
{
    public class GameInitializer : MonoBehaviour
    {
        private static readonly List<Vector2Int> spawnHexes = new()
        {
            new Vector2Int(2, -23),
            new Vector2Int(27, -23),
            new Vector2Int(20, -4),
            new Vector2Int(8, 20)
        };
        [Header("Castle Settings")]
        [SerializeField]
        private int startGold = 300;
        [SerializeField]
        private int startSupply = 10;
        [SerializeField]
        private int startHp = 500;

        [Header("Scene References")]
        [SerializeField]
        private CastleUI castleUI;
        [SerializeField]
        private WaveNotificationUI waveNotificationUI;
        [SerializeField]
        private HintUI startGameHintUI;
        [SerializeField]
        private TickManager tickManager;
        [SerializeField]
        private TowerViewManager towerViewManager;
        [SerializeField]
        private ProjectileViewManager projectileViewManager;
        [SerializeField]
        private CameraSetup cameraSetup;
        [SerializeField]
        private EndGameMenu endGameMenu;

        [Header("Unit Settings")]
        [SerializeField]
        private UnitData soldierData;
        [SerializeField]
        private UnitViewManager unitViewManager;

        [Header("Field")]
        [SerializeField]
        private FieldGenerator fieldGenerator;
        [SerializeField]
        private Tilemap tilemap;

        [Header("Monster Settings")]
        [SerializeField]
        private List<MonsterData> availableMonsters;
        [SerializeField]
        private MonsterViewManager monsterViewManager;
        [SerializeField]
        private List<WaveData> waves;
        [SerializeField]
        private float wavesDelay = 5f;

        [Header("Trap Settings")]
        [SerializeField]
        private TrapViewManager trapViewManager;

        [Header("Audio")]
        [SerializeField]
        private SoundData soundData;

        [Header("Tutorial")]
        [SerializeField]
        private TutorialManager tutorialManager;

        [Header("Infinite Mode")]
        [SerializeField]
        private InfiniteModeSettings infiniteSettings;

        [Header("CameraShake Settings")]
        [SerializeField]
        private float magnitudeScale = 20f;
        [SerializeField]
        private CameraShaker cameraShaker;
        private CastleModel castleModel;
        private CastleSystem castleSystem;
        private CastleView castleView;
        private Field.Field field;
        private GameFlowManager gameFlowManager;

        private bool gameStarted;
        private MonsterSpawner monsterSpawner;

        private MonsterSystem monsterSystem;
        private ProjectileSystem projectileSystem;
        private ShopPriceManager shopPriceManager;
        private TowersModel towersModel;
        private TowerSystem towerSystem;
        private TrapsModel trapsModel;
        private TrapSystem trapSystem;
        private UnitSystem unitSystem;
        private WaveManager waveManager;

        private void Awake()
        {
            field = fieldGenerator.GetFieldFromAsset();

            if (field == null)
            {
                Debug.LogError("Level file not found! Game cannot start.");
                return;
            }

            cameraSetup.FitToGrid();

            if (fieldGenerator != null)
                fieldGenerator.Initialize(field);

            castleModel = new CastleModel(startHp, startGold, startSupply, soundData);
            monsterSystem = new MonsterSystem();
            projectileSystem = new ProjectileSystem(monsterSystem, soundData);
            unitSystem = new UnitSystem(monsterSystem, field, tilemap, soundData);
            castleSystem = new CastleSystem(castleModel, unitSystem, soldierData, field, tilemap, soundData);

            trapsModel = new TrapsModel();
            trapSystem = new TrapSystem(monsterSystem, trapsModel, field, castleSystem, soundData);

            monsterSpawner = new MonsterSpawner(spawnHexes, field, monsterSystem, unitSystem, waves, tilemap,
                trapSystem, soundData);

            waveManager = new WaveManager(monsterSpawner, monsterSystem, wavesDelay);

            if (endGameMenu != null)
            {
                endGameMenu.Initialize(castleModel, waveManager);
                waveManager.OnGameWon += endGameMenu.OpenWinMenu;
            }

            castleView = FindAnyObjectByType<CastleView>();

            if (castleUI != null)
                castleUI.Initialize(castleSystem, waveManager);

            if (castleView != null)
                castleView.Initialize(castleModel, tilemap, field);

            if (waveNotificationUI != null)
            {
                waveNotificationUI.Initialize(waves.Count);
                waveManager.OnWaveStarting += waveNotificationUI.ShowWaveNotification;
            }

            towersModel = new TowersModel();
            towerSystem = new TowerSystem(castleSystem, towersModel, monsterSystem, projectileSystem, soundData);

            var allPriceLabels = new List<ShopPriceLabel>(FindObjectsByType<ShopPriceLabel>());
            shopPriceManager = new ShopPriceManager(castleModel, allPriceLabels);

            if (tickManager != null)
            {
                tickManager.OnTick += castleSystem.Tick;
                tickManager.OnTick += towerSystem.Tick;
                tickManager.OnTick += unitSystem.Tick;
                tickManager.OnTick += monsterSystem.Tick;
                tickManager.OnTick += monsterSpawner.Tick;
                tickManager.OnTick += projectileSystem.Tick;
                tickManager.OnTick += waveManager.Tick;
                tickManager.OnTick += trapSystem.Tick;
            }

            monsterSystem.OnMonsterDied += monster => { castleSystem.AddGold(monster.GoldReward); };
            monsterSystem.SubscribeToCastle(castleModel);

            if (cameraShaker != null)
            {
                castleModel.OnDamaged += damage =>
                {
                    var intensity = Mathf.Clamp(damage / magnitudeScale, 0.1f, 0.5f);
                    cameraShaker.Shake(0.2f, intensity);
                };
            }
        }

        private void Start()
        {
            if (unitViewManager != null)
                unitViewManager.Initialize(unitSystem);

            if (towerViewManager != null)
                towerViewManager.Initialize(towersModel);

            if (monsterViewManager != null)
                monsterViewManager.Initialize(monsterSystem);

            if (projectileViewManager != null)
                projectileViewManager.Initialize(projectileSystem);

            if (trapViewManager != null)
                trapViewManager.Initialize(trapsModel, field, tilemap);

            var sessionController =
                new SessionController(towerSystem, trapSystem, castleSystem, waveManager, infiniteSettings);

            var isLoaded = false;
            if (SessionSaveManager.IsSaveLoaded && SessionSaveManager.HasSavedSession())
            {
                StartCoroutine(sessionController.LoadStateRoutine());
                towerViewManager.SyncWithModel();
                isLoaded = true;
                castleUI.SyncBuildingsUI(castleSystem.Model.Buildings);
            }

            gameFlowManager = new GameFlowManager(
                waveManager,
                towerSystem,
                trapSystem,
                castleSystem,
                startGameHintUI
            );

            gameFlowManager.Initialize();
            if (tutorialManager != null)
            {
                if (isLoaded)
                    tutorialManager.ForceStopTutorial();
                else
                    tutorialManager.Setup(gameFlowManager);
            }

            foreach (var item in FindObjectsByType<ShopToFieldTowerItem>())
                item.Construct(towerSystem);

            foreach (var slot in FindObjectsByType<DropSlot>())
                slot.Construct(castleSystem);

            foreach (var item in FindObjectsByType<ShopToFieldTrapItem>())
                item.Construct(trapSystem, field);

            if (startGameHintUI != null)
                startGameHintUI.Initialize();
        }

        private void OnDestroy()
        {
            shopPriceManager?.Cleanup();

            if (tickManager != null)
            {
                tickManager.OnTick -= castleSystem.Tick;
                tickManager.OnTick -= towerSystem.Tick;
                tickManager.OnTick -= unitSystem.Tick;
                tickManager.OnTick -= monsterSystem.Tick;
                tickManager.OnTick -= monsterSpawner.Tick;
                tickManager.OnTick -= projectileSystem.Tick;
                tickManager.OnTick -= waveManager.Tick;
                tickManager.OnTick -= trapSystem.Tick;
            }

            castleModel?.Cleanup();
        }
    }
}