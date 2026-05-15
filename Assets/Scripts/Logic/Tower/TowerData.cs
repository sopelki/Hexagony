using Interfaces;
using Misc;
using Logic.Projectile;
using UnityEngine;

namespace Logic.Tower
{
    [CreateAssetMenu(menuName = "Towers/Tower Data")]
    public class TowerData : ScriptableObject, ITooltipProvider
    {
        public TowerType type;
        public int baseCost;
        public float range;
        public float fireRate;
        public ProjectileData projectileData;
        public GameObject viewPrefab;
        public int targetsCount = 1;
        [TextArea]
        public string description;

        public TooltipContent GetTooltipContent()
        {
            // #EF5350 - Красный (Урон)
            // #26C6DA - Голубой (Дальность)
            // #FFA726 - Оранжевый (Скорость)
            // #AB47BC - Фиолетовый (Цели)

            var stats =
                $"<color=#EF5350>Урон: {projectileData.damage}</color>\n" +
                $"<color=#AB47BC>Целей: {targetsCount}</color>\n" +
                $"<color=#FFA726>Скорость: {fireRate}</color>\n" +
                $"<color=#26C6DA>Дальность: {range}</color>";

            return new TooltipContent
            {
                Title = $"<color=#FFD700><b>{type.GetRussianName()}</b></color>",
                Description = $"<color=#BDBDBD>{description}</color>",
                Cost = $"<color=#FFEE58>Цена: {baseCost} золота</color>",
                SpecialInfo = stats
            };
        }
    }
}