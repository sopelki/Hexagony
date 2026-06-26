using System.Collections;
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
        private Color enoughGoldColor = new Color32(218, 187, 57, 255);
        [SerializeField]
        private Color notEnoughGoldColor = new Color32(178, 0, 13, 255);

        [Header("Animation")]
        [SerializeField]
        private float fadeDuration = 0.2f;

        private Coroutine colorCoroutine;

        private IPurchasable purchasable;

        public void Refresh(int currentGold)
        {
            if (purchasable == null)
                return;

            var targetColor = currentGold >= purchasable.BaseCost
                ? enoughGoldColor
                : notEnoughGoldColor;

            if (colorCoroutine != null)
                StopCoroutine(colorCoroutine);
            colorCoroutine = StartCoroutine(AnimateColor(targetColor));
        }

        private void Awake()
        {
            purchasable = data as IPurchasable;
            if (purchasable == null)
            {
                Debug.LogError($"Object {data.name} does not implement IPurchasable!");
                return;
            }

            priceText.text = purchasable.BaseCost.ToString();
        }

        private IEnumerator AnimateColor(Color targetColor)
        {
            var startColor = priceText.color;
            var elapsed = 0f;

            if (startColor == targetColor)
                yield break;

            while (elapsed < fadeDuration)
            {
                if (Time.timeScale > 0)
                    elapsed += Time.unscaledDeltaTime;

                var t = elapsed / fadeDuration;

                var easedT = t * t * (3f - 2f * t);

                priceText.color = Color.Lerp(startColor, targetColor, easedT);
                yield return null;
            }

            priceText.color = targetColor;
            colorCoroutine = null;
        }
    }
}