using Interfaces;
using Misc;
using UnityEngine;

namespace Logic.Castle
{
    [CreateAssetMenu(menuName = "Buildings/Building Data")]
    public class BuildingData : ScriptableObject, ITooltipProvider, IPurchasable
    {
        public BuildingType type;
        public int baseProduction;
        public int baseCost;
        public int supplyProvided;

        [Header("Buff Settings")]
        [Range(0f, 1f)]
        public float buffValue = 0.25f;

        public GameObject viewPrefab;

        [TextArea]
        public string description;

        [Header("Localisation & Effects")]
        [SerializeField]
        private string effectLabel = "Производство ресурсов";

        [TextArea(2, 5)]
        [SerializeField]
        private string customSpecialInfo;
        public int BaseCost => baseCost;

        public TooltipContent GetTooltipContent(bool isBought = false)
        {
            var priceInfo = isBought
                ? string.Empty
                : $"Цена: <color=#FFEE58>{baseCost} золота</color>";

            string stats;
            if (!string.IsNullOrWhiteSpace(customSpecialInfo))
                stats = customSpecialInfo;
            else
            {
                stats = type switch
                {
                    BuildingType.Blacksmith or BuildingType.Hospital =>
                        $"{effectLabel}: <color=#66BB6A>+{buffValue * 100f}%</color>",
                    BuildingType.Farm => $"{effectLabel}: <color=#66BB6A>+{supplyProvided}</color>",
                    _ => $"{effectLabel}: <color=#66BB6A>+{baseProduction}</color>"
                };
            }

            return new TooltipContent
            {
                Title = type.GetRussianName(),
                Description = description,
                Cost = priceInfo,
                SpecialInfo = stats
            };
        }
    }
}