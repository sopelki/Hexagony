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

        public void Initialize(CastleModel castleModel)
        {
            model = castleModel;
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
            hpText.text = $"<color=#479E4C>{Math.Max(0, model.Hp)}</color>";
            goldText.text = $"<color=#FFEE58>{model.Gold}</color>";
            foodText.text = $"<color=#CD7F32>{model.Food}</color>";     
        }
    }
}