using Interfaces;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ShopPriceLabel : MonoBehaviour
    {
        [SerializeField]
        private ScriptableObject data;
        [SerializeField]
        private TextMeshProUGUI priceText;

        [Header("Colors")]
        [SerializeField]
        private Color enoughGoldColor = new(218, 187, 57);
        [SerializeField]
        private Color notEnoughGoldColor = new(178, 0, 13);

        private IPurchasable purchasable;

        private void Awake()
        {
            purchasable = data as IPurchasable;
            if (purchasable == null)
                Debug.LogError($"Object {data.name} does not implement IPurchasable!");
        }

        public void Refresh(int currentGold)
        {
            if (purchasable == null)
                return;

            priceText.text = purchasable.BaseCost.ToString();

            priceText.color = currentGold >= purchasable.BaseCost
                ? enoughGoldColor
                : notEnoughGoldColor;
        }
    }
}