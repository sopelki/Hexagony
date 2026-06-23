using Interfaces;
using Logic.Projectile;
using Misc;
using UnityEngine;

namespace Logic.Tower
{
    [CreateAssetMenu(menuName = "Towers/Tower Data")]
    public class TowerData : ScriptableObject, ITooltipProvider, IPurchasable
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

        [Header("Upgrading")]
        public float damageMultiplierPerLevel = 1.75f;
        public float rangeMultiplierPerLevel = 1.1f;
        public float fireRateMultiplierPerLevel = 1.25f;
        public int maxLevel = 5;
        public int BaseCost => baseCost;

        public TooltipContent GetTooltipContent(bool isBought = false)
        {
            var stats =
                $"Урон: <color=#EF5350>{projectileData.damage}</color> [<color=#EF5350>+{damageMultiplierPerLevel * 100 % 100}%</color>]\n" +
                $"Целей: <color=#AB47BC>{targetsCount}</color>\n" +
                $"Скорость: <color=#FF7733>{fireRate}с</color> [<color=#FF7733>+{fireRateMultiplierPerLevel * 100 % 100}%</color>]\n" +
                $"Дальность: <color=#FFA726>{range}</color> [<color=#FFA726>+{rangeMultiplierPerLevel * 100 % 100}%</color>]";

            return new TooltipContent
            {
                Title = type.GetRussianName(),
                Description = description,
                SpecialInfo = stats
            };
        }
    }
}