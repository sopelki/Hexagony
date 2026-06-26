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
        [Header("Stat Rows")]
        [SerializeField]
        private StatRow hpRow;
        [SerializeField]
        private StatRow goldRow;
        [SerializeField]
        private StatRow foodRow;
        [SerializeField]
        private StatRow waveRow;

        [Header("Text Values (TMPro)")]
        [SerializeField]
        private TextMeshProUGUI hpText;
        [SerializeField]
        private TextMeshProUGUI goldText;
        [SerializeField]
        private TextMeshProUGUI foodText;
        [SerializeField]
        private TextMeshProUGUI waveText;

        [Header("Inventory Settings")]
        [SerializeField]
        private GameObject inventoryItemPrefab;

        [Header("Animation Settings")]
        [SerializeField]
        private float shakeDuration = 0.15f;
        [SerializeField]
        private float iconPunchScale = 1.05f;
        [SerializeField]
        private float labelPunchScale = 1.02f;
        [SerializeField]
        private float valuePunchScale = 1.15f;

        [Header("HP Flash Effects")]
        [SerializeField]
        private Color damageColor = new(0.8f, 0.2f, 0.2f);
        [SerializeField]
        private float flashDuration = 0.16f;

        private readonly Dictionary<RectTransform, Coroutine> activeShakes = new();

        private CastleSystem castleSystem;

        private Coroutine flashCoroutine;

        private int lastGold;
        private int lastMaxSupply;
        private int lastUnits;
        private int lastWave;
        private CastleModel model;
        private Color originalColor;
        private WaveManager waveManager;

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
            if (waveManager != null)
                waveManager.OnWaveStarting -= UpdateWaveUi;
        }

        public void Initialize(CastleSystem castleSystem, WaveManager waveManager)
        {
            model = castleSystem.CastleModel;
            this.castleSystem = castleSystem;
            this.waveManager = waveManager;

            lastGold = model.Gold;
            lastUnits = castleSystem.CurrentUnitsCount;
            lastMaxSupply = model.MaxSupply;
            lastWave = waveManager.CurrentWaveNumber;

            model.OnChanged += UpdateUI;
            waveManager.OnWaveStarting += UpdateWaveUi;
            model.OnDamaged += HandleDamage;

            UpdateUI();
        }

        private void ShakeGroup(StatRow row)
        {
            if (row == null) return;

            ShakeElement(row.icon, iconPunchScale);
            ShakeElement(row.label, labelPunchScale);
            ShakeElement(row.value, valuePunchScale);
        }

        private void ShakeElement(RectTransform target, float punchAmount)
        {
            if (target == null) return;

            if (activeShakes.TryGetValue(target, out var routine) && routine != null)
                StopCoroutine(routine);

            activeShakes[target] = StartCoroutine(SingleElementShakeRoutine(target, punchAmount));
        }

        private IEnumerator SingleElementShakeRoutine(RectTransform target, float punchAmount)
        {
            var elapsed = 0f;
            var originalScale = Vector3.one;
            var punchScale = new Vector3(punchAmount, punchAmount, punchAmount);

            while (elapsed < shakeDuration)
            {
                if (Time.timeScale == 0)
                {
                    target.localScale = originalScale;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                var t = elapsed / shakeDuration;

                target.localScale = Vector3.Lerp(punchScale, originalScale, t);
                yield return null;
            }

            target.localScale = originalScale;
            activeShakes[target] = null;
        }

        public void UpdateWaveUi(int waveNumber)
        {
            if (waveNumber != lastWave)
            {
                ShakeGroup(waveRow);
                lastWave = waveNumber;
            }
            waveText.text = $"{waveNumber}";
        }

        private void UpdateUI()
        {
            if (model.Gold != lastGold)
            {
                ShakeGroup(goldRow);
                lastGold = model.Gold;
            }

            var currentUnits = castleSystem.CurrentUnitsCount;
            if (currentUnits != lastUnits || model.MaxSupply != lastMaxSupply)
            {
                ShakeGroup(foodRow);
                lastUnits = currentUnits;
                lastMaxSupply = model.MaxSupply;
            }

            var hpPercent = model.MaxHp > 0 ? (int)Math.Round((double)Math.Max(0, model.Hp) / model.MaxHp * 100) : 0;
            if (hpPercent == 0 && model.Hp > 0) hpPercent = 1;

            hpText.text = $"{hpPercent}%";
            goldText.text = model.Gold.ToString();
            foodText.text = $"{currentUnits}/{model.MaxSupply}";
            waveText.text = $"{waveManager.CurrentWaveNumber}";

            if (model.Hp <= 0)
                hpText.color = damageColor;
            else if (flashCoroutine == null)
                hpText.color = originalColor;
        }

        private void HandleDamage(int damage)
        {
            ShakeGroup(hpRow);

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashHpRoutine());
        }

        private IEnumerator FlashHpRoutine()
        {
            var elapsed = 0f;
            var originalScale = Vector3.one;
            var punchScale = new Vector3(1.1f, 1.1f, 1.1f);

            while (elapsed < flashDuration)
            {
                if (Time.timeScale > 0) elapsed += Time.unscaledDeltaTime;

                var t = elapsed / flashDuration;
                hpText.color = Color.Lerp(damageColor, originalColor, t);
                hpText.transform.localScale = Vector3.Lerp(punchScale, originalScale, t);
                yield return null;
            }

            hpText.color = model.Hp <= 0 ? damageColor : originalColor;
            hpText.transform.localScale = originalScale;
            flashCoroutine = null;
        }

        public void SyncBuildingsUI(List<BuildingModel> savedBuildings)
        {
            var allSlots = FindObjectsByType<DropSlot>();
            for (var i = 0; i < savedBuildings.Count; i++)
            {
                if (i >= allSlots.Length) break;

                var buildingData = savedBuildings[i].Data;
                var container = allSlots[i].transform.Find("ItemContainer");
                if (container == null) continue;

                var itemGo = Instantiate(inventoryItemPrefab, container);
                var item = itemGo.GetComponent<InventoryItem>();

                if (item != null)
                {
                    item.SetData(buildingData, false);
                    item.ApplyBuildingVisual(buildingData);
                    item.Place(container);
                }
            }
        }

        [Serializable]
        public class StatRow
        {
            public RectTransform icon;
            public RectTransform label;
            public RectTransform value;
        }
    }
}