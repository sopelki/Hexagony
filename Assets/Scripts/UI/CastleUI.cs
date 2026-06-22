using System;
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

        [Header("Effects")]
        [SerializeField]
        private Color damageColor = new(0.8f, 0.2f, 0.2f);
        [SerializeField]
        private float flashDuration = 0.125f;

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

        private void HandleDamage(int damage)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashHpRoutine());
        }

        private IEnumerator FlashHpRoutine()
        {
            var elapsed = 0f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / flashDuration;
                hpText.color = Color.Lerp(damageColor, originalColor, t);

                yield return null;
            }

            hpText.color = originalColor;
            flashCoroutine = null;
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