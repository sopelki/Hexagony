using Core;
using UnityEngine;
using Interfaces;

namespace Logic.Monster
{
    public class MonsterModel : IDamageable
    {
        public int TargetedByUnits;

        public Vector3 WorldPosition { get; private set; }
        public Vector2Int CurrentHex { get; private set; }
        public Vector3 CurrentVelocity { get; private set; }
        public MonsterDebuffSystem DebuffSystem { get; }

        public bool IsDead => currentHealth <= 0;

        public readonly MonsterData Data;

        public int MaxHealth { get; }
        public int Damage => baseDamage;
        public float MoveSpeed =>
            DebuffSystem.ModifyMoveSpeed(baseMoveSpeed);
        public float AttackRadius { get; }
        public float AttackCooldown { get; }
        public int GoldReward { get; }

        private int currentHealth;
        private float baseMoveSpeed;
        private readonly int baseDamage;

        private IMovementStrategy movementStrategy;
        private IAttackStrategy attackStrategy;

        public MonsterModel(
            Vector3 startWorldPos,
            Vector2Int startHex,
            MonsterData data,
            float healthMultiplier,
            float damageMultiplier,
            float speedMultiplier)
        {
            WorldPosition = startWorldPos;
            CurrentHex = startHex;
            Data = data;

            MaxHealth = Mathf.RoundToInt(data.maxHealth * healthMultiplier);
            baseDamage = Mathf.RoundToInt(data.damage * damageMultiplier);
            baseMoveSpeed = data.moveSpeed * speedMultiplier;

            AttackRadius = data.attackRadius;
            AttackCooldown = data.attackCooldown;
            GoldReward = data.goldReward;
            DebuffSystem = new MonsterDebuffSystem(this);

            currentHealth = MaxHealth;
        }

        public void Tick()
        {
            if (IsDead)
            {
                CurrentVelocity = Vector3.zero;
                return;
            }

            CurrentVelocity = Vector3.zero;
            
            DebuffSystem.Tick(TickManager.Instance.tickInterval);
            attackStrategy?.Tick();

            if (attackStrategy?.IsAttacking == true)
                return;

            movementStrategy?.Tick();
        }

        public void Move(Vector3 direction)
        {
            var step = TickManager.Instance.tickInterval;

            WorldPosition += direction * (MoveSpeed * step);
            CurrentVelocity = direction * MoveSpeed;
        }

        public void SetHex(Vector2Int hex) => CurrentHex = hex;

        public void TakeDamage(int damage) => currentHealth -= damage;

        public void SetStrategies(IMovementStrategy movement, IAttackStrategy attack)
        {
            movementStrategy = movement;
            attackStrategy = attack;
        }
    }
}