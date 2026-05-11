using UnityEngine;

namespace Logic.Trap
{
    using UnityEngine;

    namespace Logic.Trap
    {
        [CreateAssetMenu(menuName = "Trap/Trap Data")]
        public class TrapData : ScriptableObject
        {
            public TrapType trapType;

            [Header("Visual")]
            public GameObject viewPrefab;   

            [Header("Damage Zone")]
            public float tickInterval;
            public int tickDamage;

            [Header("Slow Zone")]
            [Range(0f, 1f)]
            public float slowPercent;

            [Header("Bear Trap")]
            public int criticalDamage;
            public int requiredMonsters = 3;
            public int triggerRadius = 5;
        }
    }
}