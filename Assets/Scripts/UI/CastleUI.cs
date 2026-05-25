using System;
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

        private CastleModel model;
        private CastleSystem castleSystem;

        public void Initialize(CastleSystem castleSystem)
        {
            model = castleSystem.Model;
            this.castleSystem = castleSystem;
            model.OnChanged += UpdateUI;
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (model != null)
                model.OnChanged -= UpdateUI;
        }

        private void UpdateUI()
        {
            hpText.text = Math.Max(0, model.Hp).ToString();
            goldText.text = model.Gold.ToString();
            foodText.text = $"{castleSystem.CurrentUnitsCount} / {model.MaxSupply}";
        }
    }
}