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
        public GameObject viewPrefab;
        [TextArea]
        public string description;

        [Header("Localisation")]
        [SerializeField]
        private string effectLabel = "Производство ресурсов";

        public TooltipContent GetTooltipContent()
        {
            // #FFD700 - Золотой
            // #FFEE58 - Светло-желтый
            // #66BB6A - Зеленый

            return new TooltipContent
            {
                Title = $"<color=#FFD700><b>{type.GetRussianName()}</b></color>",
                Description = $"<color=#BDBDBD>{description}</color>",
                Cost = $"<color=#FFEE58>Цена: {baseCost} золота</color>",
                SpecialInfo = $"<color=#66BB6A>{effectLabel}: +{baseProduction}</color>"
            };
        }
    }
}