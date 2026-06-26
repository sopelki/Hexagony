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
            string stats;
            if (!string.IsNullOrWhiteSpace(customSpecialInfo))
                stats = customSpecialInfo;
            else
            {
                stats = type switch
                {
                    BuildingType.Blacksmith or BuildingType.Hospital =>
                        $"{effectLabel}: <color=#acbf67>+{buffValue * 100f}%</color>",
                    BuildingType.Farm => $"{effectLabel}: <color=#acbf67>+{supplyProvided}</color>",
                    _ => $"{effectLabel}: <color=#acbf67>+{baseProduction}</color>"
                };
            }

            return new TooltipContent
            {
                Title = type.GetRussianName(),
                Description = description,
                SpecialInfo = stats
            };
        }
    }
}