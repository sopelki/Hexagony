using System;
using System.Collections;
using System.Linq;
using Audio;
using Logic.Castle;
using Logic.Monster;
using Logic.Tower;
using Logic.Trap;
using UI;
using UnityEngine;

namespace SaveSystem
{
    public class SessionController
    {
        public SessionController(TowerSystem towerSystem, TrapSystem trapSystem, CastleSystem castleSystem,
            WaveManager waveManager, InfiniteModeSettings infiniteSettings, CastleUI castleUI)
        {
            this.towerSystem = towerSystem;
            this.trapSystem = trapSystem;
            this.castleSystem = castleSystem;
            this.waveManager = waveManager;
            this.infiniteSettings = infiniteSettings;
            this.castleUI = castleUI;

            this.waveManager.OnWaveCleared += SaveCurrentState;
        }

        private readonly CastleSystem castleSystem;
        private readonly CastleUI castleUI;
        private readonly InfiniteModeSettings infiniteSettings;
        private readonly TowerSystem towerSystem;
        private readonly TrapSystem trapSystem;
        private readonly WaveManager waveManager;

        public IEnumerator LoadStateRoutine()
        {
            var data = SessionSaveManager.LoadSession();
            if (data == null) yield break;

            if (AudioManager.Instance != null) AudioManager.Instance.MuteSfx = true;

            castleSystem.CastleModel.Gold = int.MaxValue;

            try
            {
                foreach (var b in data.buildings)
                    castleSystem.TryBuyBuilding(GameData.Instance.GetBuildingData(b.type));

                foreach (var t in data.towers)
                {
                    towerSystem.TryPlaceTower(GameData.Instance.GetTowerData(t.type), t.gridPosition, t.worldPosition,
                        t.level);
                }

                foreach (var trap in data.traps)
                    trapSystem.TryPlaceTrap(GameData.Instance.GetTrapData(trap.type), trap.centerHex);

                castleSystem.CastleModel.Hp = data.castleHp;
                castleSystem.CastleModel.Gold = data.gold;
                castleSystem.CastleModel.Changed();
                castleUI.UpdateWaveUi(data.currentWaveNumber);
                waveManager.StartSavedGame(data.currentWaveNumber, infiniteSettings);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during loading: {e.Message}");
            }

            yield return null;
            yield return null;
            yield return null;

            if (AudioManager.Instance != null) AudioManager.Instance.MuteSfx = false;

            Debug.Log("Session loaded. Silence mode finished after 3 frames.");
        }

        private void SaveCurrentState(int currentWave)
        {
            var data = new GameSessionData
            {
                currentWaveNumber = currentWave,
                castleHp = castleSystem.CastleModel.Hp,
                gold = castleSystem.CastleModel.Gold
            };

            foreach (var tower in towerSystem.GetTowers())
            {
                data.towers.Add(new TowerSaveData
                {
                    type = tower.Data.type,
                    level = tower.Level,
                    gridPosition = tower.GridPosition,
                    worldPosition = tower.WorldPosition
                });
            }

            foreach (var trap in trapSystem.GetTraps().Where(trap => trap.Hexes.Count > 0))
                data.traps.Add(new TrapSaveData { type = trap.Data.trapType, centerHex = trap.Hexes[0] });

            foreach (var building in castleSystem.CastleModel.Buildings)
                data.buildings.Add(new BuildingSaveData { type = building.Data.type });

            SessionSaveManager.SaveSession(data);
        }
    }
}