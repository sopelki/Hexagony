using System;
using Audio;
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
        public int Damage { get; }
        public float MoveSpeed => DebuffSystem.ModifyMoveSpeed(baseMoveSpeed);
        public float AttackRadius { get; }
        public float AttackCooldown { get; }
        public int GoldReward { get; }
        
        public Vector3 HitPosition => WorldPosition + new Vector3(0, Data.hitOffsetY, 0);

        private int currentHealth;
        private readonly float baseMoveSpeed;

        private IMovementStrategy movementStrategy;
        private IAttackStrategy attackStrategy;
        private readonly SoundData soundData;
        
        private bool deathEventSent;
        
        public event Action OnAttack;
        public event Action OnDamaged; 
        public event Action OnDied;

        public MonsterModel(
            Vector3 startWorldPos,
            Vector2Int startHex,
            MonsterData data,
            float healthMultiplier,
            float damageMultiplier,
            float speedMultiplier,
            SoundData soundData)
        {
            WorldPosition = startWorldPos;
            CurrentHex = startHex;
            Data = data;

            MaxHealth = Mathf.RoundToInt(data.maxHealth * healthMultiplier);
            Damage = Mathf.RoundToInt(data.damage * damageMultiplier);
            baseMoveSpeed = data.moveSpeed * speedMultiplier;

            AttackRadius = data.attackRadius;
            AttackCooldown = data.attackCooldown;
            GoldReward = data.goldReward;
            DebuffSystem = new MonsterDebuffSystem(this);

            currentHealth = MaxHealth;
            
            this.soundData = soundData;
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

        public void TakeDamage(int damage)
        {
            if (IsDead) 
                return;
            currentHealth -= damage;
            
            OnDamaged?.Invoke();
            
            if (soundData != null && 
                soundData.monsterDamageSounds is { Length: > 0 })
                AudioManager.Instance.PlayRandomSfx(soundData.monsterDamageSounds, soundData.monsterDamageVolume);
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void Attack()
        {
            OnAttack?.Invoke();
        }

        public void Die()
        {
            if (deathEventSent) 
                return;
            
            deathEventSent = true;
            OnDied?.Invoke();
        }

        public void SetStrategies(IMovementStrategy movement, IAttackStrategy attack)
        {
            movementStrategy = movement;
            attackStrategy = attack;
        }
    }
}