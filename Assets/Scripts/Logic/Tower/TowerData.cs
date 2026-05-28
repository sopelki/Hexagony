using Interfaces;
using Logic.Projectile;
using Misc;
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

        public TooltipContent GetTooltipContent(bool isBought = false)
        {
            var stats =
                $"Урон: <color=#EF5350>{projectileData.damage}</color>\n" +
                $"Целей: <color=#AB47BC>{targetsCount}</color>\n" +
                $"Скорость: <color=#FF7733>{fireRate}с</color>\n" +
                $"Дальность: <color=#FFA726>{range}</color>";

            return new TooltipContent
            {
                Title = type.GetRussianName(),
                Description = description,
                Cost = $"Цена: <color=#FFEE58>{baseCost} золота</color>",
                SpecialInfo = stats
            };
        }
    }
}