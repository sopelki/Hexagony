using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    public class WaveNotificationUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI waveText;
        [SerializeField]
        private TextMeshProUGUI shadowText;
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private float displayDuration = 3f;
        [SerializeField]
        private float fadeDuration = 0.5f;
        [SerializeField]
        private float targetOpacity = 0.75f;

        private int wavesCount;
        private Coroutine displayCoroutine;

        public void Initialize(int wavesCount)
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0;

            this.wavesCount = wavesCount;
        }

        public void ShowWaveNotification(int waveNumber)
        {
            if (displayCoroutine != null)
                StopCoroutine(displayCoroutine);

            displayCoroutine = StartCoroutine(DisplayWaveCoroutine(waveNumber));
        }

        private IEnumerator DisplayWaveCoroutine(int waveNumber)
        {
            var text = $"Началась волна {waveNumber} из {wavesCount}";
            if (waveText)
                waveText.text = text;

            if (shadowText)
                shadowText.text = text;

            yield return FadeCanvasGroup(0, targetOpacity, fadeDuration);

            yield return new WaitForSeconds(displayDuration);

            yield return FadeCanvasGroup(targetOpacity, 0, fadeDuration);
        }

        private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration)
        {
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
        }
    }
}