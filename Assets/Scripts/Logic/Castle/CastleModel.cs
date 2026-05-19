using System;
using System.Collections.Generic;
using Audio;
using Interfaces;
using UnityEngine;

namespace Logic.Castle
{
    public class CastleModel : IDamageable
    {
        public int Hp { get; set; }
        public int Gold { get; set; }
        public int Food { get; set; }
        public int CurrentUnits { get; set; }

        public List<Vector2Int> WallHexes { get; set; } = new();
        public List<Vector3> WallWorldPositions { get; set; } = new();

        public List<BuildingModel> Buildings { get; private set; } = new();

        public event Action OnChanged;
        
        private readonly SoundData soundData;
        
        public void Changed()
        {
            Debug.Log("Castle Changed");
            OnChanged?.Invoke();
        }

        public CastleModel(int initialHp, int initialGold, int initialFood, SoundData soundData)
        {
            Hp = initialHp;
            Gold = initialGold;
            Food = initialFood;
            this.soundData = soundData;
        }

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
    }
}