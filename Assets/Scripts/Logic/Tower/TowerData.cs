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

        public TooltipContent GetTooltipContent(bool isBought = false)
        {
            var stats =
                $"Урон: <color=#EF5350>{projectileData.damage}</color>\n" +
                $"Целей: <color=#AB47BC>{targetsCount}</color>\n" +
                $"Скорость: <color=#FFA726>{fireRate}с</color>\n" +
                $"Дальность: <color=#26C6DA>{range}</color>";

            return new TooltipContent
            {
                Title = $"<color=#FFD700><b>{type.GetRussianName()}</b></color>",
                Description = $"<color=#BDBDBD>{description}</color>",
                Cost = $"Цена: <color=#FFEE58>{baseCost} золота</color>",
                SpecialInfo = stats
            };
        }
    }
}