using Interfaces;
using Misc;
using UnityEngine;

namespace Logic.Trap
{
    [CreateAssetMenu(menuName = "Trap/Trap Data")]
    public class TrapData : ScriptableObject, ITooltipProvider
    {
        public TrapType trapType;

        [Header("Cost")]
        public int baseCost;

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
        public int requiredMonsters = 1;
        public int triggerRadius = 5;

        [Header("Tooltip")]
        [TextArea]
        public string description;

        public TooltipContent GetTooltipContent(bool isBought = false)
        {
            var stats = trapType switch
            {
                TrapType.DamageZone => $"Урон: <color=#EF5350>{tickDamage}</color>\n" +
                                       $"Интервал: <color=#FF7733>{tickInterval}с</color>",

                TrapType.SlowZone => $"Замедление: <color=#26C6DA>{slowPercent * 100f}%</color>\n" +
                                     $"Интервал: <color=#FF7733>{tickInterval}с</color>",

                TrapType.BearTrap => $"Крит. урон: <color=#EF5350>{criticalDamage}</color>\n" +
                                     $"Радиус: <color=#26C6DA>{triggerRadius}</color>\n",

                _ => $"Урон: <color=#EF5350>{tickDamage}</color>"
            };

            return new TooltipContent
            {
                Title = trapType.GetRussianName(),
                Description = description,
                Cost = $"Цена: <color=#FFEE58>{baseCost} золота</color>",
                SpecialInfo = stats
            };
        }
    }
}