using System.Linq;
using Audio;
using Core;
using Interfaces;
using Logic.Castle;
using Logic.Unit;
using UnityEngine;

namespace Logic.Monster
{
    public class MonsterAttackStrategy : IAttackStrategy
    {
        private readonly MonsterModel monsterModel;
        private readonly SoundData soundData;
        private readonly UnitSystem unitSystem;

        private float currentCooldown;
        private IDamageable currentTarget;

        public MonsterAttackStrategy(
            MonsterModel monsterModel,
            UnitSystem unitSystem,
            SoundData soundData)
        {
            this.monsterModel = monsterModel;
            this.unitSystem = unitSystem;
            this.soundData = soundData;
        }

        public bool IsAttacking => currentTarget != null;

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
                currentCooldown -= TickManager.Instance.tickInterval;
                return;
            }

            monsterModel.Attack();

            if (soundData != null && soundData.monsterAttackSounds is { Length: > 0 })
                AudioManager.Instance.PlayRandomSfx(soundData.monsterAttackSounds, soundData.monsterAttackVolume);

            if (monsterModel.Data.attackType == AttackType.SingleClosest)
                currentTarget.TakeDamage(monsterModel.Damage);
            else if (monsterModel.Data.attackType == AttackType.Radius)
            {
                var targetPos = currentTarget is UnitModel u ? u.WorldPosition : monsterModel.WorldPosition;
                if (currentTarget is CastleModel)
                {
                    targetPos = castle.Model.WallWorldPositions
                        .OrderBy(p => Vector3.Distance(monsterModel.WorldPosition, p))
                        .First();
                }

                var viewDirection = (targetPos - monsterModel.WorldPosition).normalized;
                if (viewDirection.sqrMagnitude < 0.001f)
                    viewDirection = (targetPos - monsterModel.WorldPosition).normalized;


                var allUnitsInRadius = unitSystem.GetAllUnits()
                    .Where(unitModel => !unitModel.IsDead)
                    .Where(unitModel => Vector3.Distance(unitModel.WorldPosition, monsterModel.WorldPosition) <=
                                        monsterModel.AttackRadius);

                foreach (var unitModel in allUnitsInRadius)
                {
                    var directionToUnit = (unitModel.WorldPosition - monsterModel.WorldPosition).normalized;
                    var dotProduct = Vector3.Dot(viewDirection, directionToUnit);

                    if (dotProduct >= 0.5f || unitModel == currentTarget)
                        unitModel.TakeDamage(monsterModel.Damage);
                }
                if (currentTarget is CastleModel)
                    currentTarget.TakeDamage(monsterModel.Damage);
            }
            currentCooldown = monsterModel.AttackCooldown;
        }
    }
}