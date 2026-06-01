using System;
using System.Collections.Generic;
using Audio;
using Interfaces;
using UnityEngine;

namespace Logic.Castle
{
    public class CastleModel : IDamageable
    {
        private readonly SoundData soundData;
        public event Action OnCastleDestroyed;

        public CastleModel(int initialHp, int initialGold, int startSupply, SoundData soundData)
        {
            Hp = initialHp;
            MaxHp = initialHp;
            Gold = initialGold;
            this.soundData = soundData;
            MaxSupply = startSupply;
        }

        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Gold { get; set; }
        public int MaxSupply { get; set; }

        public List<Vector2Int> WallHexes { get; set; } = new();
        public List<Vector3> WallWorldPositions { get; set; } = new();

        public List<BuildingModel> Buildings { get; private set; } = new();

        public bool IsDead => Hp <= 0;

        public void TakeDamage(int damage)
        {
            if (IsDead)
                return;
            Debug.Log("Castle Damage took " + damage);
            Hp -= damage;
            Changed();

            if (soundData != null &&
                soundData.castleDamageSounds is { Length: > 0 })
                AudioManager.Instance.PlayRandomSfx(soundData.castleDamageSounds, soundData.castleDamageVolume);

            if (Hp <= 0)
                OnCastleDestroyed?.Invoke();
        }

        public event Action OnChanged;


        public void Changed()
        {
            Debug.Log("Castle Changed");
            OnChanged?.Invoke();
        }
    }
}