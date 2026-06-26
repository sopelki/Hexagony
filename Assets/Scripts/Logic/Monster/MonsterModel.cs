using System;
using Audio;
using Core;
using Interfaces;
using Logic.Trap;
using UnityEngine;

namespace Logic.Monster
{
    public class MonsterModel : IDamageable
    {
        public MonsterModel(Vector3 startWorldPos, Vector2Int startHex, MonsterData data, float healthMultiplier,
            float damageMultiplier, float speedMultiplier, float goldMultiplier, SoundData soundData)
        {
            WorldPosition = startWorldPos;
            CurrentHex = startHex;
            Data = data;

            MaxHealth = Mathf.RoundToInt(data.maxHealth * healthMultiplier);
            Damage = Mathf.RoundToInt(data.damage * damageMultiplier);
            baseMoveSpeed = data.moveSpeed * speedMultiplier;

            AttackRadius = data.attackRadius;
            AttackCooldown = data.attackCooldown;
            GoldReward = Mathf.RoundToInt(data.goldReward * goldMultiplier);
            DebuffSystem = new MonsterDebuffSystem(this);

            currentHealth = MaxHealth;

            this.soundData = soundData;
        }

        public readonly MonsterData Data;

        public int TargetedByUnits;

        private IAttackStrategy attackStrategy;

        private readonly float baseMoveSpeed;

        private int currentHealth;

        private bool deathEventSent;

        private IMovementStrategy movementStrategy;
        private readonly SoundData soundData;

        public float AttackCooldown { get; }
        public float AttackRadius { get; }
        public Vector2Int CurrentHex { get; private set; }
        public Vector3 CurrentVelocity { get; private set; }
        public int Damage { get; }
        public MonsterDebuffSystem DebuffSystem { get; }
        public int GoldReward { get; }

        public Vector3 HitPosition => WorldPosition + new Vector3(0, Data.hitOffsetY, 0);

        public bool IsDead => currentHealth <= 0;

        public int MaxHealth { get; }
        public float MoveSpeed => DebuffSystem.ModifyMoveSpeed(baseMoveSpeed);

        public Vector3 WorldPosition { get; private set; }

        public event Action OnAttack;
        public event Action OnDamaged;
        public event Action OnDied;

        public void Attack()
        {
            OnAttack?.Invoke();
        }

        public void Die()
        {
            if (deathEventSent) return;

            deathEventSent = true;
            OnDied?.Invoke();
        }

        public bool HasImmunity(TrapType trapType)
        {
            // return Data.immuneTraps != null && Data.immuneTraps.Contains(trapType);
            var immune = Data.immuneTraps != null && Data.immuneTraps.Contains(trapType);
            Debug.Log($"Проверка иммунитета для {Data.name} к {trapType}: {immune}");
            return immune;
        }

        public void Move(Vector3 direction)
        {
            var step = TickManager.Instance.tickInterval;

            WorldPosition += direction * (MoveSpeed * step);
            CurrentVelocity = direction * MoveSpeed;
        }

        public void SetHex(Vector2Int hex)
        {
            CurrentHex = hex;
        }

        public void SetPosition(Vector3 newPosition)
        {
            WorldPosition = newPosition;
        }

        public void SetStrategies(IMovementStrategy movement, IAttackStrategy attack)
        {
            movementStrategy = movement;
            attackStrategy = attack;
        }

        public void StopAndIdle()
        {
            movementStrategy = null;
            attackStrategy = null;
            CurrentVelocity = Vector3.zero;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return;
            currentHealth -= damage;

            OnDamaged?.Invoke();

            if (soundData != null && soundData.monsterDamageSounds is { Length: > 0 })
                AudioManager.Instance.PlayRandomSfx(soundData.monsterDamageSounds, soundData.monsterDamageVolume);
            if (currentHealth <= 0) Die();
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

            if (attackStrategy?.IsAttacking == true) return;

            movementStrategy?.Tick();
        }
    }
}