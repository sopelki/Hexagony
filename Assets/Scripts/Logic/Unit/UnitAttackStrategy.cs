using System.Linq;
using Audio;
using Core;
using Interfaces;
using Logic.Monster;
using UnityEngine;

namespace Logic.Unit
{
    public class UnitAttackStrategy : IAttackStrategy
    {
        private readonly MonsterSystem monsterSystem;
        private readonly SoundData soundData;
        private readonly UnitModel unit;

        private float currentCooldown;

        public UnitAttackStrategy(
            UnitModel unit,
            MonsterSystem monsterSystem,
            SoundData soundData)
        {
            this.unit = unit;
            this.monsterSystem = monsterSystem;
            this.soundData = soundData;
        }

        public bool IsAttacking { get; private set; }

        public void Tick()
        {
            if (currentCooldown > 0f)
            {
                currentCooldown -= TickManager.Instance.tickInterval;
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

            var sortedTargets = targets
                .OrderBy(m => Vector3.Distance(m.WorldPosition, unit.WorldPosition))
                .ToList();

            var closest = sortedTargets.First();
            var distance = Vector3.Distance(closest.WorldPosition, unit.WorldPosition);
            var meleeRange = unit.UnitData.attackRadius * 0.5f;
            unit.AttackType = distance <= meleeRange ? 1 : 2;

            unit.Attack();

            if (soundData != null)
            {
                if (distance <= meleeRange &&
                    soundData.unitMeleeAttackSounds is { Length: > 0 })
                {
                    AudioManager.Instance.PlayRandomSfx(soundData.unitMeleeAttackSounds,
                        soundData.unitMeleeAttackVolume);
                }
                else if (distance > meleeRange &&
                         soundData.unitRangeAttackSounds is { Length: > 0 })
                {
                    AudioManager.Instance.PlayRandomSfx(soundData.unitRangeAttackSounds,
                        soundData.unitRangeAttackVolume);
                }
            }

            var viewDirection = unit.CurrentDirection.normalized;
            if (viewDirection.sqrMagnitude < 0.001f)
                viewDirection = (closest.WorldPosition - unit.WorldPosition).normalized;

            foreach (var monster in targets)
            {
                var directionToMonster = (monster.WorldPosition - unit.WorldPosition).normalized;
                var dotProduct = Vector3.Dot(viewDirection, directionToMonster);
                if (dotProduct >= 0.5f || monster == closest)
                    monster.TakeDamage(unit.GetAttack());
            }

            currentCooldown = unit.UnitData.attackCooldown;
            IsAttacking = true;
        }
    }
}