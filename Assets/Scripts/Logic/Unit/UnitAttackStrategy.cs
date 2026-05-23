using System.Linq;
using Interfaces;
using UnityEngine;
using Logic.Monster;
using Audio;

namespace Logic.Unit
{
    public class UnitAttackStrategy : IAttackStrategy
    {
        private readonly UnitModel unit;
        private readonly MonsterSystem monsterSystem;
        private readonly SoundData soundData;

        private float currentCooldown;

        public bool IsAttacking { get; private set; }

        public UnitAttackStrategy(
            UnitModel unit,
            MonsterSystem monsterSystem,
            SoundData soundData)
        {
            this.unit = unit;
            this.monsterSystem = monsterSystem;
            this.soundData = soundData;
        }

        public void Tick()
        {
            if (currentCooldown > 0f)
            {
                currentCooldown -= Core.TickManager.Instance.tickInterval;
                IsAttacking = true;
                return;
            }
            
            IsAttacking = false;
            unit.AttackType = 0;

            var targets = monsterSystem.GetAllMonsters()
                .Where(m => !m.IsDead)
                .Where(m =>
                    Vector3.Distance(
                        m.WorldPosition,
                        unit.WorldPosition) <= unit.UnitData.attackRadius)
                .ToList();

            if (targets.Count == 0)
                return;
        
            var closest = targets
                .OrderBy(m => Vector3.Distance(m.WorldPosition, unit.WorldPosition))
                .First();
        
            var distance = Vector3.Distance(closest.WorldPosition, unit.WorldPosition);
            var meleeRange = unit.UnitData.attackRadius * 0.5f;
            unit.AttackType = distance <= meleeRange ? 1 : 2;
            
            unit.Attack();

            if (soundData != null)
            {
                if (distance <= meleeRange && 
                    soundData.unitMeleeAttackSounds is { Length: > 0 })
                    AudioManager.Instance.PlayRandomSfx(soundData.unitMeleeAttackSounds, soundData.unitMeleeAttackVolume);
                else if (distance > meleeRange && 
                         soundData.unitRangeAttackSounds is { Length: > 0 })
                    AudioManager.Instance.PlayRandomSfx(soundData.unitRangeAttackSounds, soundData.unitRangeAttackVolume);
            }

            foreach (var monster in targets)
                monster.TakeDamage(unit.GetAttack());

            currentCooldown = unit.UnitData.attackCooldown;
            IsAttacking = true;
        }
    }
}