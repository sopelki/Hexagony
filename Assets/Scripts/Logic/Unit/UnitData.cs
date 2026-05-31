using UnityEngine;

namespace Logic.Unit
{
    [CreateAssetMenu(menuName = "Units/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [Header("Stats")]
        public int maxHealth = 100;
        public int attack = 10;
        public float moveSpeed = 6f;
        // public int foodCost = 5;

        [Header("Attack")]
        public float attackRadius = 1f;
        public float attackCooldown = 1f;

        [Header("View")]
        public GameObject unitPrefab;
        public float visualOffsetY;
    }
}