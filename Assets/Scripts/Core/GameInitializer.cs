using System.Collections.Generic;
using Field;
using Logic.Castle;
using Logic.Monster;
using Logic.Projectile;
using Logic.Tower;
using Logic.Unit;
using View;
using UI;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        // [SerializeField] private CastleView castleView;
        [SerializeField]
        private TickManager tickManager;
        [SerializeField]
        private TowerViewManager towerViewManager;
        [SerializeField]
        private ProjectileViewManager projectileViewManager;
        [SerializeField]
        private CameraSetup cameraSetup;
        [SerializeField] private MenuScripts.GameOverMenu gameOverMenu;

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
        private static readonly List<Vector2Int> spawnHexes = new()
        {
            new Vector2Int(2, -23),
            new Vector2Int(27, -23),
            new Vector2Int(20, -4),
            new Vector2Int(8, 20),
        };

        // private static readonly Vector2Int castleHex = new(-30, 20);

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
            
            castleModel = new CastleModel(startHp, startGold, startFood);
            monsterSystem = new MonsterSystem();
            projectileSystem = new ProjectileSystem(monsterSystem);
            unitSystem = new UnitSystem(
                monsterSystem,
                field,
                tilemap
            );
            
            castleSystem = new CastleSystem(
                castleModel,
                unitSystem,
                soldierData,
                field,
                tilemap
            );

            
            castleView = FindAnyObjectByType<CastleView>();
            
            if (castleUI != null)
                castleUI.Initialize(castleModel);
            
            if (castleView != null)
                castleView.Initialize(castleModel, tilemap, field);
            else 
                Debug.LogWarning("GameInitializer: No castle found in objects. Check level JSON or ObjectMappings.");
            
            if (gameOverMenu != null)
                gameOverMenu.Initialize(castleModel);

            monsterSpawner = new MonsterSpawner(
                spawnHexes,
                field,
                monsterSystem,
                unitSystem,
                availableMonsters,
                tilemap
            );
            
            towersModel = new TowersModel();
            towerSystem = new TowerSystem(castleSystem, towersModel, monsterSystem, projectileSystem);

            if (tickManager == null)
                return;

            tickManager.OnTick += castleSystem.Tick;
            tickManager.OnTick += towerSystem.Tick;
            tickManager.OnTick += unitSystem.Tick;
            tickManager.OnTick += monsterSystem.Tick;
            tickManager.OnTick += monsterSpawner.Tick;
            tickManager.OnTick += projectileSystem.Tick;

            unitSystem.OnUnitDied += _ =>
            {
                castleModel.CurrentUnits--;
                castleModel.Changed();
            };
            
            monsterSystem.OnMonsterDied += monster => 
            {
                castleSystem.AddGold(monster.Data.goldReward);
                Debug.Log($"Monster is killed. Gold received: {monster.Data.goldReward}. Balance: {castleModel.Gold}");
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

            var shopItems = FindObjectsByType<ShopToFieldItem>();
            foreach (var item in shopItems)
                item.Construct(towerSystem);

            var slots = FindObjectsByType<DropSlot>();
            foreach (var slot in slots)
                slot.Construct(castleSystem);
        }
    }
}