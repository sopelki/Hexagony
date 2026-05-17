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
        public int requiredMonsters = 3;
        public int triggerRadius = 5;

        [Header("Tooltip")]
        [TextArea]
        public string description;

        public TooltipContent GetTooltipContent(bool isBought = false)
        {
            // Используем те же цвета, что и в TowerData:
            // #EF5350 - Красный (Урон)
            // #26C6DA - Голубой (Замедление / Радиус)
            // #FFA726 - Оранжевый (Интервал)
            // #AB47BC - Фиолетовый (Спец. инфо)

            var stats = trapType switch
            {
                TrapType.DamageZone => $"Урон: <color=#EF5350>{tickDamage}</color>\n" +
                                       $"Интервал: <color=#FFA726>{tickInterval}с</color>",

                TrapType.SlowZone => $"Замедление: <color=#26C6DA>{slowPercent * 100f}%</color>\n" +
                                     $"Интервал: <color=#FFA726>{tickInterval}с</color>",

                TrapType.BearTrap => $"Крит. урон: <color=#EF5350>{criticalDamage}</color>\n" +
                                     $"Радиус: <color=#26C6DA>{triggerRadius}</color>\n" +
                                     $"Нужно монстров: <color=#AB47BC>{requiredMonsters}</color>",

                _ => $"Урон: <color=#EF5350>{tickDamage}</color>"
            };

            return new TooltipContent
            {
                Title = $"<color=#FFD700><b>{trapType.GetRussianName()}</b></color>",
                Description = $"<color=#BDBDBD>{description}</color>",
                Cost = $"Цена: <color=#FFEE58>{baseCost} золота</color>",
                SpecialInfo = stats
            };
        }
    }
}