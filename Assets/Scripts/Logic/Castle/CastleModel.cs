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

        public CastleModel(int initialHp, int initialGold, int startSupply, SoundData soundData)
        {
            Hp = initialHp;
            Gold = initialGold;
            this.soundData = soundData;
            MaxSupply = startSupply;
        }

        public int Hp { get; set; }
        public int Gold { get; set; }
        // public int Food { get; set; }
        // public int CurrentUnits { get; set; }
        public int MaxSupply { get; set; }

        public List<Vector2Int> WallHexes { get; set; } = new();
        public List<Vector3> WallWorldPositions { get; set; } = new();

        public List<BuildingModel> Buildings { get; private set; } = new();

        public bool IsDead => Hp <= 0;

        public void TakeDamage(int damage)
        {
            Debug.Log("Castle Damage took " + damage);
            Hp -= damage;
            Changed();

            if (soundData != null &&
                soundData.castleDamageSounds is { Length: > 0 })
                AudioManager.Instance.PlayRandomSfx(soundData.castleDamageSounds, soundData.castleDamageVolume);
        }

        public event Action OnChanged;

        public void Changed()
        {
            Debug.Log("Castle Changed");
            OnChanged?.Invoke();
        }
    }
}