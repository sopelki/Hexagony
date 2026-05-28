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
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (currentFade != null) StopCoroutine(currentFade);
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

            if (currentFade != null)
                StopCoroutine(currentFade);

            currentFade = StartCoroutine(FadeRoutine(0f, duration));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            var startAlpha = canvasGroup.alpha;
            var startTime = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - startTime < duration)
            {
                var elapsed = Time.realtimeSinceStartup - startTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }
    }
}