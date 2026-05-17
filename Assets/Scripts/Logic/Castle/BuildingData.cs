using Interfaces;
using Misc;
using UnityEngine;

namespace Logic.Castle
{
    [CreateAssetMenu(menuName = "Buildings/Building Data")]
    public class BuildingData : ScriptableObject, ITooltipProvider
    {
        public BuildingType type;
        public int baseProduction;
        public int baseCost;

        [Header("Buff Settings")]
        [Range(0f, 1f)]
        public float buffValue = 0.25f;

        public GameObject viewPrefab;

        [TextArea]
        public string description;

        [Header("Localisation & Effects")]
        [SerializeField]
        private string effectLabel = "Производство ресурсов";

        [Tooltip("Если заполнено, заменяет блок характеристик полностью")]
        [TextArea(2, 5)]
        [SerializeField]
        private string customSpecialInfo;

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
                stats = type is BuildingType.Blacksmith or BuildingType.Hospital
                    ? $"{effectLabel}: <color=#66BB6A>+{buffValue * 100f}%</color>"
                    : $"{effectLabel}: <color=#66BB6A>+{baseProduction}</color>";
            }

            return new TooltipContent
            {
                Title = $"<color=#FFD700><b>{type.GetRussianName()}</b></color>",
                Description = $"<color=#BDBDBD>{description}</color>",
                Cost = priceInfo,
                SpecialInfo = stats
            };
        }
    }
}