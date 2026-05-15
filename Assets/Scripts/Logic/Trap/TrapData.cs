using Interfaces;

namespace Logic.Trap
{
    using UnityEngine;

    namespace Logic.Trap
    {
        [CreateAssetMenu(menuName = "Trap/Trap Data")]
        public class TrapData : ScriptableObject, ITooltipProvider
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
            private ITooltipProvider tooltipProviderImplementation;

            [Header("Tooltip")]
            public string description;

            // TODO: допилить инфу
            public TooltipContent GetTooltipContent(bool isBought = false)
            {
                return new TooltipContent
                {
                    Title = trapType.ToString(),
                    Description = description,
                    Cost = $"Цена: ",
                    SpecialInfo = $"Урон: "
                };
            }
        }
    }
}