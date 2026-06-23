using System.Collections;
using UnityEngine;

namespace MenuScripts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadePanel : MonoBehaviour
    {
        [SerializeField]
        private float fadeDuration = 0.05f;
        private CanvasGroup canvasGroup;
        private Coroutine currentFade;

        public float FadeDuration => fadeDuration;
        public float CurrentAlpha => canvasGroup != null ? canvasGroup.alpha : 0f;

        private void Awake()
        {
            EnsureCanvasGroup();
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            Show(fadeDuration);
        }

        public void Show(float duration)
        {
            EnsureCanvasGroup();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (canvasGroup.alpha >= 1f && currentFade == null) return;

            if (currentFade != null) StopCoroutine(currentFade);

            if (duration <= 0)
                canvasGroup.alpha = 1f;
            else
                currentFade = StartCoroutine(FadeRoutine(1f, duration));
        }

        public void Hide()
        {
            Hide(fadeDuration);
        }

        public void Hide(float duration)
        {
            EnsureCanvasGroup();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (canvasGroup.alpha <= 0f && currentFade == null)
                return;

            if (currentFade != null)
                StopCoroutine(currentFade);

            if (duration <= 0)
                canvasGroup.alpha = 0f;
            else
                currentFade = StartCoroutine(FadeRoutine(0f, duration));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            var startAlpha = canvasGroup.alpha;
            var startTime = Time.unscaledTime;

            while (Time.unscaledTime - startTime < duration)
            {
                var elapsed = Time.unscaledTime - startTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var easedT = t * t * (3f - 2f * t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, easedT);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            currentFade = null;
        }
    }
}