using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logic.Monster;
using Core;
using Logic.Trap.Logic.Trap;

namespace Logic.Trap
{
    public class TrapSystem
    {
        private readonly MonsterSystem monsterSystem;
        private readonly TrapsModel trapsModel;

        public TrapSystem(MonsterSystem monsterSystem, TrapsModel trapsModel)
        {
            this.monsterSystem = monsterSystem;
            this.trapsModel = trapsModel;
            TickManager.Instance.OnTick += Tick;
        }

        public void Dispose()
        {
            if (TickManager.Instance != null)
                TickManager.Instance.OnTick -= Tick;
        }

        public bool TryPlaceTrap(TrapData data, Vector2Int hex)
        {
            if (trapsModel.Traps.Any(t => t.Hex == hex))
                return false;

            var trap = new TrapModel(data, hex);
            trapsModel.AddTrap(trap);
            return true;
        }

        // ===============================
        // Вход монстра
        // ===============================

        public void OnMonsterEnteredCell(Vector2Int hex, MonsterModel monster)
        {
            Debug.Log($"<color=yellow>Monster entered hex {hex}</color>");
            var trap = trapsModel.Traps.FirstOrDefault(t => t.Hex == hex);
            if (trap == null || trap.IsTriggered)
                return;

            if (trap.Data.trapType == TrapType.SlowZone)
            {
                Debug.Log($"<color=cyan>Trap triggered at {hex} | Type: {trap.Data.trapType}</color>");
                if (!trap.ActiveSlowDebuffs.ContainsKey(monster))
                {
                    var slow = new SlowDebuff(trap.Data.slowPercent);

                    trap.ActiveSlowDebuffs.Add(monster, slow);
                    monster.DebuffSystem.AddBuff(slow);
                }
            }
        }

        // ===============================
        // Выход монстра
        // ===============================

        public void OnMonsterExitedCell(Vector2Int hex, MonsterModel monster)
        {
            var trap = trapsModel.Traps.FirstOrDefault(t => t.Hex == hex);
            if (trap == null)
                return;

            if (trap.Data.trapType == TrapType.SlowZone)
            {
                if (trap.ActiveSlowDebuffs.TryGetValue(monster, out var slow))
                {
                    monster.DebuffSystem.RemoveBuff(slow);
                    trap.ActiveSlowDebuffs.Remove(monster);
                }
            }
        }

        // ===============================
        // Главный Tick
        // ===============================

        private void Tick()
        {
            var delta = TickManager.Instance.tickInterval;
            var monsters = monsterSystem.GetAllMonsters();

            foreach (var trap in trapsModel.Traps.ToList())
            {
                if (trap.IsTriggered)
                    continue;

                switch (trap.Data.trapType)
                {
                    case TrapType.DamageZone:
                        HandleDamageZone(trap, monsters, delta);
                        break;

                    case TrapType.BearTrap:
                        HandleBearTrap(trap, monsters);
                        break;
                }
            }
        }

        // ===============================
        // Damage Zone
        // ===============================

        private void HandleDamageZone(TrapModel trap, IReadOnlyList<MonsterModel> monsters, float delta)
        {
            Debug.Log($"<color=red>Damage Trap tick at {trap.Hex} → Damage: {trap.Data.tickDamage}</color>");
            trap.TickTimer += delta;

            if (trap.TickTimer < trap.Data.tickInterval)
                return;

            trap.TickTimer = 0f;

            foreach (var monster in monsters)
            {
                if (!monster.IsDead && monster.CurrentHex == trap.Hex)
                {
                    monster.TakeDamage(trap.Data.tickDamage);
                }
            }
        }

        // ===============================
        // Bear Trap
        // ===============================

        private void HandleBearTrap(TrapModel trap, IReadOnlyList<MonsterModel> monsters)
        {
            
            var inRadius = monsters.Where(m =>
                !m.IsDead &&
                Vector2Int.Distance(m.CurrentHex, trap.Hex) <= trap.Data.triggerRadius
            ).ToList();
            Debug.Log($"<color=magenta>Bear Trap SNAP at {trap.Hex} | Monsters hit: {inRadius.Count}</color>");
            if (inRadius.Count >= trap.Data.requiredMonsters)
            {
                foreach (var monster in inRadius)
                {
                    monster.TakeDamage(trap.Data.criticalDamage);
                }

                trap.Trigger();
                trapsModel.RemoveTrap(trap);
            }
        }
    }
}