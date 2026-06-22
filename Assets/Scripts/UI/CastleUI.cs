using System;
using System.Collections.Generic;
using System.Collections;
using Logic.Castle;
using TMPro;
using UnityEngine;

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

        [Header("Effects")]
        [SerializeField]
        private Color damageColor = new(0.8f, 0.2f, 0.2f);
        [SerializeField]
        private float flashDuration = 0.16f;

        private Color originalColor;
        private Coroutine flashCoroutine;

        private CastleSystem castleSystem;
        private CastleModel model;

        private void Awake()
        {
            originalColor = hpText.color;
        }

        private void OnDestroy()
        {
            if (model != null)
            {
                model.OnChanged -= UpdateUI;
                model.OnDamaged -= HandleDamage;
            }
        }

        public void Initialize(CastleSystem castleSystem)
        {
            model = castleSystem.Model;
            this.castleSystem = castleSystem;

            model.OnChanged += UpdateUI;
            model.OnDamaged += HandleDamage;

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
        

        private void HandleDamage(int damage)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashHpRoutine());
        }

        private IEnumerator FlashHpRoutine()
        {
            var elapsed = 0f;
            var originalScale = Vector3.one;
            var punchScale = new Vector3(1.075f, 1.075f, 1.075f);

            while (elapsed < flashDuration)
            {
                if (Time.timeScale > 0)
                    elapsed += Time.unscaledDeltaTime;

                var t = elapsed / flashDuration;
                
                hpText.color = Color.Lerp(damageColor, originalColor, t);
                hpText.transform.localScale = Vector3.Lerp(punchScale, originalScale, t);

                yield return null;
            }

            hpText.color = model.Hp <= 0 ? damageColor : originalColor;
            hpText.transform.localScale = originalScale;
            flashCoroutine = null;
        }

        private void UpdateUI()
        {
            var hpPercent = model.MaxHp > 0 ? (int)Math.Round((double)Math.Max(0, model.Hp) / model.MaxHp * 100) : 0;
            
            hpText.text = $"{hpPercent}%";
            goldText.text = model.Gold.ToString();
            foodText.text = $"{castleSystem.CurrentUnitsCount} / {model.MaxSupply}";

            if (model.Hp <= 0)
                hpText.color = damageColor;
            
            else if (flashCoroutine == null)
                hpText.color = originalColor;
        }
    }
}