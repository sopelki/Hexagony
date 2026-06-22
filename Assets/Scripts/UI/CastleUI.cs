using System;
using System.Collections.Generic;
using Logic.Castle;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CastleUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI hpText;
        [SerializeField]
        private TextMeshProUGUI goldText;
        [SerializeField]
        private TextMeshProUGUI foodText;
        [SerializeField]
        private GameObject inventoryItemPrefab;
        private CastleSystem castleSystem;

        private CastleModel model;

        private void OnDestroy()
        {
            if (model != null)
                model.OnChanged -= UpdateUI;
        }

        public void Initialize(CastleSystem castleSystem)
        {
            model = castleSystem.Model;
            this.castleSystem = castleSystem;
            model.OnChanged += UpdateUI;
            UpdateUI();
        }
        
        public void SyncBuildingsUI(List<BuildingModel> savedBuildings)
        {
            var allSlots = FindObjectsByType<DropSlot>();
    
            for (int i = 0; i < savedBuildings.Count; i++)
            {
                if (i >= allSlots.Length) break;
        
                var buildingData = savedBuildings[i].Data;
                
                var itemGo = Instantiate(inventoryItemPrefab, allSlots[i].transform.Find("ItemContainer"));
                var item = itemGo.GetComponent<InventoryItem>();
                
                item.SetData(buildingData, false);
                item.ApplyBuildingVisual(buildingData);
                item.Place(allSlots[i].transform.Find("ItemContainer"));
            }
        }
        

        private void UpdateUI()
        {
            var hpPercent = model.MaxHp > 0 ? (int)Math.Round((double)Math.Max(0, model.Hp) / model.MaxHp * 100) : 0;

            hpText.text = $"{hpPercent}%";
            goldText.text = model.Gold.ToString();
            foodText.text = $"{castleSystem.CurrentUnitsCount} / {model.MaxSupply}";
        }
    }
}