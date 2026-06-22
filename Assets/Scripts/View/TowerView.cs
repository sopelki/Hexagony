using System.Collections; // Нужно для корутин
using TMPro;
using UnityEngine;

namespace View
{
    public class TowerView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        [SerializeField]
        private TextMeshPro levelText;
        [SerializeField]
        private TextMeshPro shadowLevelText;
        [SerializeField]
        private Color previewColor = Color.yellow;

        [Header("Animation")]
        [SerializeField]
        private float animationDuration = 0.2f;

        private Color originalColor;
        private Coroutine levelAnimation;
        private Vector3 originalTextScale;

        private void Awake()
        {
            if (levelText != null)
                originalTextScale = levelText.transform.localScale;
        }

        public void Initialize(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
            originalColor = levelText.color;
            SetLevelImmediate(1);
        }

        public void ShowPreviewLevel(int level) => StartLevelAnimation(level, previewColor);

        public void ResetPreview(int actualLevel) => StartLevelAnimation(actualLevel, originalColor);

        public void SetLevel(int level) => StartLevelAnimation(level, originalColor);

        private void StartLevelAnimation(int level, Color targetColor)
        {
            if (levelAnimation != null)
                StopCoroutine(levelAnimation);
            levelAnimation = StartCoroutine(AnimateLevelIn(level, targetColor));
        }

        private IEnumerator AnimateLevelIn(int level, Color targetColor)
        {
            var newRoman = level <= 1 ? "" : IntToRoman(level);

            if (levelText.text == newRoman)
            {
                float elapsed = 0;
                var startColor = levelText.color;
                while (elapsed < animationDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    levelText.color = Color.Lerp(startColor, targetColor, elapsed / animationDuration);

                    if (shadowLevelText)
                        shadowLevelText.color = new Color(0, 0, 0, levelText.color.a);

                    yield return null;
                }
                yield break;
            }

            levelText.text = newRoman;
            if (shadowLevelText)
                shadowLevelText.text = newRoman;

            float t = 0;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / animationDuration;

                var c = targetColor;
                c.a = Mathf.Lerp(0, targetColor.a, t);
                levelText.color = c;
                if (shadowLevelText)
                    shadowLevelText.color = new Color(0, 0, 0, c.a);

                var scaleEffect = Mathf.Sin(t * Mathf.PI * 0.5f);
                levelText.transform.localScale = originalTextScale * Mathf.Lerp(0.5f, 1f, scaleEffect);

                yield return null;
            }
        }

        private void SetLevelImmediate(int level)
        {
            var roman = level <= 1 ? "" : IntToRoman(level);
            levelText.text = roman;
            if (shadowLevelText)
                shadowLevelText.text = roman;
            levelText.color = originalColor;
            levelText.transform.localScale = originalTextScale;
        }

        private static string IntToRoman(int number)
        {
            return number switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                8 => "VIII",
                9 => "IX",
                10 => "X",
                _ => number.ToString()
            };
        }
    }
}