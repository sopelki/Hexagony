using System.Collections.Generic;
using Logic.Trap;
using UnityEngine;

namespace Logic.Monster
{
    [CreateAssetMenu(menuName = "Monsters/Monster Data")]
    public class MonsterData : ScriptableObject
    {
        [Header("Stats")]
        public int maxHealth = 100;
        public int damage = 10;
        public float moveSpeed = 3f;
        public int goldReward = 20;
        public string monsterName = "Monster";
        public string monsterDescription = "";

        [Header("Attack")]
        public float attackRadius = 1f;
        public float attackCooldown = 1f;
        public AttackType attackType = AttackType.Radius;

        [Header("Trap Immune")]
        public List<TrapType> immuneTraps;

        [Header("View")]
        public GameObject prefab;
        public float hitOffsetY;
        public float visualOffsetY;
    }
}