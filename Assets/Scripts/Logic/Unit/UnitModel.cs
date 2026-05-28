using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Interfaces;
using UnityEngine;

namespace Logic.Unit
{
    public class UnitModel : IDamageable
    {
        private readonly List<Buff> buffs = new();
        private readonly SoundData soundData;
        private IAttackStrategy attackStrategy;
        private bool deathEventSent;

        private IMovementStrategy movementStrategy;

        public UnitModel(Vector3 startPos, Vector2Int startHex, UnitData unitData, SoundData soundData)
        {
            WorldPosition = startPos;
            CurrentHex = startHex;
            UnitData = unitData;
            CurrentHealth = unitData.maxHealth;
            this.soundData = soundData;
        }

        public Vector3 WorldPosition { get; private set; }
        public Vector2Int CurrentHex { get; private set; }
        public Vector3 CurrentDirection { get; set; }
        public int AttackType { get; set; }

        public UnitData UnitData { get; }
        public IReadOnlyList<Buff> ActiveBuffs => buffs;

        public int CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public void TakeDamage(int damage)
        {
            if (IsDead) return;
            Debug.Log($"Unit got damaged: {damage}");
            OnDamaged?.Invoke();
            CurrentHealth -= damage;

            if (soundData != null &&
                soundData.unitDamageSounds is { Length: > 0 })
                AudioManager.Instance.PlayRandomSfx(soundData.unitDamageSounds, soundData.unitDamageVolume);
            if (CurrentHealth <= 0)
                Die();
        }

        public event Action OnAttack;
        public event Action OnDamaged;
        public event Action OnDied;

        public void SetStrategies(
            IMovementStrategy movement,
            IAttackStrategy attack)
        {
            movementStrategy = movement;
            attackStrategy = attack;
        }

        public void Tick()
        {
            if (IsDead)
                return;

            attackStrategy?.Tick();

            if (attackStrategy?.IsAttacking == true)
            {
                CurrentDirection = Vector3.zero;
                return;
            }

            movementStrategy?.Tick();
        }

        public float GetMoveSpeed()
        {
            return buffs.Aggregate(UnitData.moveSpeed, (current, buff) => buff.ModifyMoveSpeed(current));
        }

        public int GetMaxHealth()
        {
            return buffs.Aggregate(UnitData.maxHealth, (current, buff) => buff.ModifyMaxHealth(current));
        }

        public void ResetHealth()
        {
            CurrentHealth = GetMaxHealth();
        }

        public int GetAttack()
        {
            var result = buffs.Aggregate(UnitData.attack, (current, buff) => buff.ModifyAttack(current));
            Debug.Log($"Unit dealt damage: {result}");
            return result;
        }

        public void AddBuff(Buff buff)
        {
            buffs.Add(buff);
        }

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

        public void SetHex(Vector2Int hex)
        {
            CurrentHex = hex;
        }

        public void SetPosition(Vector3 newPosition)
        {
            WorldPosition = newPosition;
        }
    }
}