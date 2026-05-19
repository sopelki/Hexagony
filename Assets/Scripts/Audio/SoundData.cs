using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data")]
    public class SoundData : ScriptableObject
    {
        [Header("Building Sounds")]
        public AudioClip towerPlaceSound;
        public AudioClip trapPlaceSound;
        public AudioClip buildingPlaceSound;
        [Range(0f, 1f)]
        public float buildingPlacementVolume = 0.7f;

        [Header("Monster Attack Sounds")]
        public AudioClip[] monsterDamageSounds;
        [Range(0f, 1f)]
        public float monsterDamageVolume = 0.6f;
        
        public AudioClip[] monsterAttackSounds;
        [Range(0f, 1f)]
        public float monsterAttackVolume = 0.8f;

        [Header("Tower Sounds")]
        public AudioClip[] archerTowerShootSounds;
        [Range(0f, 1f)]
        public float archerShootVolume = 0.75f;
        
        public AudioClip[] archerTowerHitSounds;
        [Range(0f, 1f)]
        public float archerHitVolume = 0.6f;
        
        public AudioClip[] mageTowerShootSounds;
        [Range(0f, 1f)]
        public float mageShootVolume = 0.75f;
        
        public AudioClip[] mageTowerHitSounds;
        [Range(0f, 1f)]
        public float mageHitVolume = 0.6f;

        [Header("Unit Attack Sounds")]
        public AudioClip[] unitMeleeAttackSounds;
        [Range(0f, 1f)]
        public float unitMeleeAttackVolume = 0.8f;

        public AudioClip[] unitRangeAttackSounds;
        [Range(0f, 1f)]
        public float unitRangeAttackVolume = 0.8f;

        public AudioClip[] unitDamageSounds;
        [Range(0f, 1f)]
        public float unitDamageVolume = 0.6f;
        
        [Header("Castle damage Sounds")]
        public AudioClip[] castleDamageSounds;
        [Range(0f, 1f)]
        public float castleDamageVolume = 0.8f;

        [Header("Background Music")]
        public AudioClip backgroundMusic;

    }
}