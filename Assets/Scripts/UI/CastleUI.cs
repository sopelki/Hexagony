using System;
using System.Collections;
using System.Collections.Generic;
using Logic.Castle;
using Logic.Monster;
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
        private TextMeshProUGUI waveText;
        [SerializeField]
        private GameObject inventoryItemPrefab;

        [Header("Effects")]
        [SerializeField]
        private Color damageColor = new(0.8f, 0.2f, 0.2f);
        [SerializeField]
        private float flashDuration = 0.16f;

        private CastleSystem castleSystem;
        private WaveManager waveManager;
        private Coroutine flashCoroutine;
        private CastleModel model;

        private Color originalColor;

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

        public void Initialize(CastleSystem castleSystem, WaveManager waveManager)
        {
            model = castleSystem.CastleModel;
            this.castleSystem = castleSystem;
            this.waveManager = waveManager;

            model.OnChanged += UpdateUI;
            waveManager.OnWaveStarting += UpdateWaveUi;
            model.OnDamaged += HandleDamage;

            UpdateUI();
        }

        public void SyncBuildingsUI(List<BuildingModel> savedBuildings)
        {
            var allSlots = FindObjectsByType<DropSlot>();

            for (var i = 0; i < savedBuildings.Count; i++)
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

        public void UpdateWaveUi(int waveNumber)
        {
            Debug.Log($"Updating UI from action. Current wave: {waveNumber}");
            waveText.text = $"{waveNumber}";
        }

        private void UpdateUI()
        {
            Debug.Log($"Updating UI. Current wave: {waveManager.CurrentWaveNumber}");
            var hpPercent = model.MaxHp > 0 ? (int)Math.Round((double)Math.Max(0, model.Hp) / model.MaxHp * 100) : 0;

            if (hpPercent == 0 && model.Hp > 0)
                hpPercent = 1;

            hpText.text = $"{hpPercent}%";
            goldText.text = model.Gold.ToString();
            foodText.text = $"{castleSystem.CurrentUnitsCount} / {model.MaxSupply}";
            waveText.text = $"{waveManager.CurrentWaveNumber}";

            if (model.Hp <= 0)
                hpText.color = damageColor;

            else if (flashCoroutine == null)
                hpText.color = originalColor;
        }
    }
}