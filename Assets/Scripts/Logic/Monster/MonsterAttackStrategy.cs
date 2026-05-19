using System.Linq;
using Interfaces;
using Logic.Castle;
using UnityEngine;
using Logic.Unit;
using Audio;

namespace Logic.Monster
{
    public class MonsterAttackStrategy : IAttackStrategy
    {
        private readonly MonsterModel monsterModel;
        private readonly UnitSystem unitSystem;
        private readonly SoundData soundData;

        private float currentCooldown;
        private IDamageable currentTarget;

        public bool IsAttacking => currentTarget != null;

        public MonsterAttackStrategy(
            MonsterModel monsterModel,
            UnitSystem unitSystem,
            SoundData soundData)
        {
            this.monsterModel = monsterModel;
            this.unitSystem = unitSystem;
            this.soundData = soundData;
        }

        public void Tick()
        {
            var castle = CastleSystem.Instance;
            if (castle == null) return;

            if (currentTarget != null)
            {
                Vector3 targetPos;
                if (currentTarget is UnitModel unit)
                    targetPos = unit.WorldPosition;
                else
                {
                    if (castle.Model.WallWorldPositions.Count == 0)
                    {
                        currentTarget = null;
                        return;
                    }
                    targetPos = castle.Model.WallWorldPositions
                        .OrderBy(p => Vector3.Distance(monsterModel.WorldPosition, p))
                        .First();
                }

                if (currentTarget.IsDead ||
                    Vector3.Distance(targetPos, monsterModel.WorldPosition) > monsterModel.AttackRadius)
                    currentTarget = null;
            }

            if (currentTarget == null)
            {
                var nearbyUnit = unitSystem
                    .GetAllUnits()
                    .Where(u => !u.IsDead)
                    .FirstOrDefault(u =>
                        Vector3.Distance(u.WorldPosition, monsterModel.WorldPosition) <= monsterModel.AttackRadius);

                if (nearbyUnit != null)
                    currentTarget = nearbyUnit;
                else if (!castle.Model.IsDead && castle.Model.WallWorldPositions.Count > 0)
                {
                    foreach (var wallPos in castle.Model.WallWorldPositions)
                    {
                        if (Vector3.Distance(wallPos, monsterModel.WorldPosition) <= monsterModel.AttackRadius)
                        {
                            currentTarget = castle.Model;
                            break;
                        }
                    }
                }

                if (currentTarget == null)
                    return;
            }

            if (currentCooldown > 0f)
            {
                currentCooldown -= Core.TickManager.Instance.tickInterval;
                return;
            }

            if (soundData != null && soundData.monsterAttackSounds is { Length: > 0 })
                AudioManager.Instance.PlayRandomSfx(soundData.monsterAttackSounds, soundData.monsterAttackVolume);

            currentTarget.TakeDamage(monsterModel.Damage);
            currentCooldown = monsterModel.AttackCooldown;
        }
    }
}