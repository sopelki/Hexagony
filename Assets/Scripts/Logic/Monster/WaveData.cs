using System.Collections.Generic;
using UnityEngine;

namespace Logic.Monster
{
    [CreateAssetMenu(menuName = "Monsters/Wave Data")]
    public class WaveData : ScriptableObject
    {
        public List<MonsterData> monsterPool;

        public int totalMonsters = 10;
        public float spawnInterval = 2f;

        [Header("Multipliers")]
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
    }
}