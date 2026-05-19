using System.Collections.Generic;
using Audio;
using Field;
using Logic.Castle;
using Logic.Monster;
using Logic.Projectile;
using Logic.Tower;
using Logic.Trap;
using Logic.Unit;
using View;
using UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using View.View;

namespace Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Castle Settings")]
        [SerializeField]
        private int startGold = 300;
        [SerializeField]
        private int startFood = 100;
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
        private MenuScripts.EndGameMenu endGameMenu;

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

        private MonsterSystem monsterSystem;
        private MonsterSpawner monsterSpawner;
        private CastleModel castleModel;
        private CastleSystem castleSystem;
        private TowersModel towersModel;
        private TowerSystem towerSystem;
        private UnitSystem unitSystem;
        private CastleView castleView;
        private ProjectileSystem projectileSystem;
        private Field.Field field;
        private WaveManager waveManager;
        private TrapSystem trapSystem;
        private TrapsModel trapsModel;
        private GameFlowManager gameFlowManager;

        private bool gameStarted;

        private static readonly List<Vector2Int> spawnHexes = new()
        {
            new Vector2Int(2, -23),
            new Vector2Int(27, -23),
            new Vector2Int(20, -4),
            new Vector2Int(8, 20),
        };

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

            castleModel = new CastleModel(startHp, startGold, startFood, soundData);
            monsterSystem = new MonsterSystem();
            projectileSystem = new ProjectileSystem(monsterSystem, soundData);
            unitSystem = new UnitSystem(monsterSystem, field, tilemap, soundData);
            castleSystem = new CastleSystem(castleModel, unitSystem, soldierData, field, tilemap, soundData);

            castleView = FindAnyObjectByType<CastleView>();

            if (castleUI != null)
                castleUI.Initialize(castleModel);

            if (castleView != null)
                castleView.Initialize(castleModel, tilemap, field);

            if (endGameMenu != null)
                endGameMenu.Initialize(castleModel);


            trapsModel = new TrapsModel();
            trapSystem = new TrapSystem(monsterSystem, trapsModel, field, castleSystem, soundData);

            monsterSpawner =
                new MonsterSpawner(spawnHexes, field, monsterSystem, unitSystem, waves, tilemap, trapSystem, soundData);

            waveManager = new WaveManager(monsterSpawner, monsterSystem, wavesDelay);
            waveManager.OnGameWon += endGameMenu.OpenWinMenu;

            if (waveNotificationUI != null)
            {
                waveNotificationUI.Initialize();
                waveManager.OnWaveStarting += waveNotificationUI.ShowWaveNotification;
            }

            towersModel = new TowersModel();
            towerSystem = new TowerSystem(castleSystem, towersModel, monsterSystem, projectileSystem, soundData);

            if (tickManager == null)
                return;

            tickManager.OnTick += castleSystem.Tick;
            tickManager.OnTick += towerSystem.Tick;
            tickManager.OnTick += unitSystem.Tick;
            tickManager.OnTick += monsterSystem.Tick;
            tickManager.OnTick += monsterSpawner.Tick;
            tickManager.OnTick += projectileSystem.Tick;
            tickManager.OnTick += waveManager.Tick;
            tickManager.OnTick += trapSystem.Tick;

            unitSystem.OnUnitDied += _ =>
            {
                castleModel.CurrentUnits--;
                castleModel.Changed();
            };

            monsterSystem.OnMonsterDied += monster =>
            {
                castleSystem.AddGold(monster.GoldReward);
                Debug.Log($"Monster is killed. Gold received: {monster.GoldReward}. Balance: {castleModel.Gold}");
            };
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

            var shopItems = FindObjectsByType<ShopToFieldTowerItem>();
            foreach (var item in shopItems)
                item.Construct(towerSystem);

            var slots = FindObjectsByType<DropSlot>();
            foreach (var slot in slots)
                slot.Construct(castleSystem);

            var trapShopItems = FindObjectsByType<ShopToFieldTrapItem>();
            foreach (var item in trapShopItems)
                item.Construct(trapSystem, field);

            if (startGameHintUI != null)
                startGameHintUI.Initialize();

            gameFlowManager = new GameFlowManager(
                waveManager,
                towerSystem,
                trapSystem,
                castleSystem,
                startGameHintUI
            );

            gameFlowManager.Initialize();
        }
    }
}