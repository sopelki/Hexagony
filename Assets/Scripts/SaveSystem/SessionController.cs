using Logic.Castle;
using Logic.Monster;
using Logic.Tower;
using Logic.Trap;
using UnityEngine;

namespace SaveSystem
{
    public class SessionController
    {
        private readonly TowerSystem towerSystem;
        private readonly TrapSystem trapSystem;
        private readonly CastleSystem castleSystem;
        private readonly WaveManager waveManager;
        private readonly InfiniteModeSettings infiniteSettings;
        
        public SessionController(TowerSystem towerSystem, TrapSystem trapSystem, CastleSystem castleSystem, WaveManager waveManager, InfiniteModeSettings infiniteSettings)
        {
            this.towerSystem = towerSystem;
            this.trapSystem = trapSystem;
            this.castleSystem = castleSystem;
            this.waveManager = waveManager;
            this.infiniteSettings = infiniteSettings;
            
            this.waveManager.OnWaveCleared += SaveCurrentState;
        }
        
        private void SaveCurrentState(int currentWave)
        {
            var data = new GameSessionData
            {
                currentWaveNumber = currentWave,
                castleHp = castleSystem.Model.Hp,
                gold = castleSystem.Model.Gold
            };
            
            foreach (var tower in towerSystem.GetTowers())
                data.towers.Add(new TowerSaveData { type = tower.Data.type, level = tower.Level, gridPosition = tower.GridPosition, worldPosition = tower.WorldPosition });
            
            foreach (var trap in trapSystem.GetTraps())
            {
                if (trap.Hexes.Count > 0)
                    data.traps.Add(new TrapSaveData { type = trap.Data.trapType, centerHex = trap.Hexes[0] });
            }
            
            foreach (var building in castleSystem.Model.Buildings)
                data.buildings.Add(new BuildingSaveData { type = building.Data.type });

            SessionSaveManager.SaveSession(data);
        }
        
        public void LoadState()
        {
            var data = SessionSaveManager.LoadSession();
            if (data == null) return;
            
            castleSystem.Model.Gold = 999999; 
            
            foreach (var b in data.buildings)
                castleSystem.TryBuyBuilding(GameData.Instance.GetBuildingData(b.type));
            
            foreach (var t in data.towers)
                towerSystem.TryPlaceTower(GameData.Instance.GetTowerData(t.type), t.gridPosition, t.worldPosition, t.level);
            
            foreach (var trap in data.traps)
                trapSystem.TryPlaceTrap(GameData.Instance.GetTrapData(trap.type), trap.centerHex);
            
            castleSystem.Model.Changed();
            Canvas.ForceUpdateCanvases();
            castleSystem.Model.Hp = data.castleHp;
            castleSystem.Model.Gold = data.gold;
            castleSystem.Model.Changed();

            waveManager.StartSavedGame(data.currentWaveNumber, infiniteSettings);
        }
    }
}