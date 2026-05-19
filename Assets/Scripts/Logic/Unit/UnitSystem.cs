using System;
using System.Collections.Generic;
using Audio;
using Logic.Monster;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Logic.Unit
{
    public class UnitSystem
    {
        private readonly List<UnitModel> units = new();
        private readonly MonsterSystem monsterSystem;
        private readonly Field.Field field;
        private readonly Tilemap tilemap;
        private readonly List<Buff> buffs = new();
        private readonly SoundData soundData;

        public event Action<UnitModel> OnUnitCreated;
        // public event Action<UnitModel> OnUnitMoved;
        public event Action<UnitModel> OnUnitDied;

        public UnitSystem(
            MonsterSystem monsterSystem,
            Field.Field field,
            Tilemap tilemap,
            SoundData soundData)
        {
            this.monsterSystem = monsterSystem;
            this.field = field;
            this.tilemap = tilemap;
            this.soundData = soundData;
        }
        
        public void AddBuff(Buff buff) => buffs.Add(buff);

        public void SpawnUnit(Vector3 worldPos, Vector2Int hexPos, UnitData stats)
        {
            var unit = new UnitModel(worldPos, hexPos, stats, soundData);
            
            foreach (var buff in buffs)
                unit.AddBuff(buff);
            
            unit.ResetHealth();
            
            var attack = new UnitAttackStrategy(unit, monsterSystem, soundData);
            var movement = new UnitAStarMoveStrategy(
                unit,
                monsterSystem,
                field,
                tilemap
            );

            unit.SetStrategies(movement, attack);
            units.Add(unit);
            
            Debug.Log($"[UnitSpawn] HP: {unit.CurrentHealth}/{unit.GetMaxHealth()} | ATK: {unit.GetAttack()} | Buffs: {buffs.Count}");

            OnUnitCreated?.Invoke(unit);
        }

        // TODO: Переделать в foreach
        public void Tick()
        {
            for (var i = units.Count - 1; i >= 0; i--)
            {
                var unit = units[i];

                if (unit.IsDead)
                {
                    Debug.Log("Unit died");
                    units.RemoveAt(i);
                    OnUnitDied?.Invoke(unit);
                    continue;
                }

                unit.Tick();
            }
        }
        
        public IReadOnlyList<UnitModel> GetAllUnits() => units;

        public void Clear() => units.Clear();
    }
}