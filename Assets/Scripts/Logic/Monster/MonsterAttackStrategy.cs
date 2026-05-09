using System.Linq;
using Interfaces;
using UnityEngine;
using Logic.Unit;
using View;

namespace Logic.Monster
{
    public class MonsterAttackStrategy : IAttackStrategy
    {
        private readonly MonsterModel monster;
        private readonly UnitSystem unitSystem;
        private readonly CastleView castleView;

        private float currentCooldown;
        private IDamageable currentTarget;

        public bool IsAttacking => currentTarget != null;

        public MonsterAttackStrategy(
            MonsterModel monster,
            UnitSystem unitSystem,
            CastleView castleView)
        {
            this.monster = monster;
            this.unitSystem = unitSystem;
            this.castleView = castleView;
        }

        public void Tick()
        {
            if (currentTarget != null)
            {
                Vector3 targetPos;
                if (currentTarget is UnitModel unit) 
                    targetPos = unit.WorldPosition;
                else 
                    targetPos = castleView.GetClosestWallPoint(monster.WorldPosition);
                
                if (currentTarget.IsDead || Vector3.Distance(targetPos, monster.WorldPosition) > monster.Data.attackRadius)
                    currentTarget = null;
            }

            if (currentTarget == null)
            {
                var nearbyUnit = unitSystem.GetAllUnits()
                    .Where(u => !u.IsDead)
                    .FirstOrDefault(u => Vector3.Distance(u.WorldPosition, monster.WorldPosition) <= monster.Data.attackRadius);

                if (nearbyUnit != null)
                    currentTarget = nearbyUnit;
                // 2. Ищем, не подошли ли мы к ЛЮБОМУ из гексов замка
                else if (!castleView.Model.IsDead)
                {
                    foreach (var wallPos in castleView.WallWorldPositions)
                    {
                        if (Vector3.Distance(wallPos, monster.WorldPosition) <= monster.Data.attackRadius)
                        {
                            currentTarget = castleView.Model;
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

            currentTarget.TakeDamage(monster.Data.damage);
            currentCooldown = monster.Data.attackCooldown;
        }
    }
}