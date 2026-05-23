using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logic.Monster;
using Core;
using Logic.Castle;
using HexagonScripts;
using Audio;

namespace Logic.Trap
{
    public class TrapSystem
    {
        public event Action OnFirstTrapPlaced;
        
        private readonly MonsterSystem monsterSystem;
        private readonly TrapsModel trapsModel;
        private readonly Field.Field field;
        private readonly CastleSystem castleSystem;
        private readonly SoundData soundData;
        
        private bool firstTrapPlaced;

        public TrapSystem(MonsterSystem monsterSystem, TrapsModel trapsModel, Field.Field field,
            CastleSystem castleSystem,  SoundData soundData)
        {
            this.field = field;
            this.monsterSystem = monsterSystem;
            this.trapsModel = trapsModel;
            this.castleSystem = castleSystem;
            this.soundData = soundData;
            // TickManager.Instance.OnTick += Tick;
        }

        public List<Vector2Int> GetTrapOccupiedHexes(Vector2Int centerHex)
        {
            return new List<Vector2Int>
            {
                centerHex,
                centerHex + new Vector2Int(0, -1),
                centerHex + new Vector2Int(1, -1)
            };
        }

        public bool CanPlaceTrap(TrapData data, Vector2Int axial)
        {
            if (castleSystem.Model.Gold < data.baseCost) return false;
            var hexes = GetTrapOccupiedHexes(axial);

            foreach (var h in hexes)
            {
                var hexObj = field.GetHex(h);

                if (hexObj == null)
                    return false;
                if (hexObj.type != HexagonType.Path)
                    return false;

                if (trapsModel.Traps.Any(t => t.Hexes.Contains(h)))
                    return false;
            }
            return true;
        }

        public bool TryPlaceTrap(TrapData data, Vector2Int hex)
        {
            if (!CanPlaceTrap(data, hex))
                return false;
            
            var occupied = GetTrapOccupiedHexes(hex);
            
            Debug.Log($"Placing trap at center AXIAL: {hex}");
            foreach (var h in occupied)
                Debug.Log($"Trap occupies AXIAL: {h}");

            if (castleSystem.TrySpendGold(data.baseCost))
            {
                var trap = new TrapModel(data, GetTrapOccupiedHexes(hex));
                trap.OnTriggered += HandleTrapTriggered;
                trapsModel.AddTrap(trap);
                
                if (soundData != null && soundData.trapPlaceSound != null)
                    AudioManager.Instance.PlaySfx(soundData.trapPlaceSound);
                
                if (!firstTrapPlaced)
                {
                    firstTrapPlaced = true;
                    OnFirstTrapPlaced?.Invoke();
                    Debug.Log("First trap placed. Game can start.");
                }
                
                return true;
            }
            
            return false;
        }
        
        private void HandleTrapTriggered(TrapModel trap)
        {
            trap.OnTriggered -= HandleTrapTriggered;
            trapsModel.RemoveTrap(trap);
        }

        public void OnMonsterEnteredCell(Vector2Int hex, MonsterModel monster)
        {
            var trap = trapsModel.Traps.FirstOrDefault(t => t.Hexes.Contains(hex));
            if (trap == null || trap.IsTriggered) return;

            if (trap.Data.trapType == TrapType.SlowZone)
            {
                if (!trap.ActiveSlowDebuffs.ContainsKey(monster))
                {
                    var slow = new SlowDebuff(trap.Data.slowPercent);
                    trap.ActiveSlowDebuffs.Add(monster, slow);
                    monster.DebuffSystem.AddBuff(slow);
                }
            }
        }

        public void OnMonsterExitedCell(Vector2Int hex, MonsterModel monster)
        {
            var trap = trapsModel.Traps.FirstOrDefault(t => t.Hexes.Contains(hex));
            if (trap == null) return;
            if (trap.Hexes.Contains(monster.CurrentHex)) return;

            if (trap.Data.trapType == TrapType.SlowZone && trap.ActiveSlowDebuffs.TryGetValue(monster, out var slow))
            {
                monster.DebuffSystem.RemoveBuff(slow);
                trap.ActiveSlowDebuffs.Remove(monster);
            }
        }

        public void Tick()
        {
            var delta = TickManager.Instance.tickInterval;
            var monsters = monsterSystem.GetAllMonsters();
            foreach (var trap in trapsModel.Traps.ToList().Where(trap => !trap.IsTriggered))
            {
                switch (trap.Data.trapType)
                {
                    case TrapType.DamageZone:
                        HandleDamageZone(trap, monsters, delta);
                        break;
                    case TrapType.BearTrap:
                        HandleBearTrap(trap, monsters);
                        break;
                    case TrapType.SlowZone:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void HandleDamageZone(TrapModel trap, IReadOnlyList<MonsterModel> monsters, float delta)
        {
            trap.TickTimer += delta;
            if (trap.TickTimer < trap.Data.tickInterval) return;
            while (trap.TickTimer >= trap.Data.tickInterval)
            {
                trap.TickTimer -= trap.Data.tickInterval;
                foreach (var monster in monsters)
                {
                    if (!monster.IsDead && trap.Hexes.Contains(monster.CurrentHex))
                        monster.TakeDamage(trap.Data.tickDamage);
                }
            }
        }

        private void HandleBearTrap(TrapModel trap, IReadOnlyList<MonsterModel> monsters)
        {
            var inZone = monsters.Where(m => !m.IsDead && trap.Hexes.Contains(m.CurrentHex)).ToList();
            if (inZone.Count >= trap.Data.requiredMonsters)
            {
                foreach (var m in inZone) m.TakeDamage(trap.Data.criticalDamage);
                trap.Trigger();
                Debug.Log("Bear trap triggered.");
                trapsModel.RemoveTrap(trap);
            }
        }
    }
}