using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    public class SlidingNotificationUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private TextMeshProUGUI textElement;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Position Settings")]
        [SerializeField] private Vector2 hiddenPosition = new(0, 200);
        [SerializeField] private Vector2 visiblePosition = new(0, -100);

        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve entranceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve exitCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Durations")]
        [SerializeField] private float slideInDuration = 0.6f;
        [SerializeField] private float slideOutDuration = 0.3f;
        [SerializeField] private float displayDuration = 3f;

        private Coroutine activeCoroutine;
        private int totalWavesCount;

        public float DisplayDuration => displayDuration;

        public void Initialize(int totalWaves = 0)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (panelRect == null) panelRect = GetComponent<RectTransform>();

            totalWavesCount = totalWaves;
            panelRect.anchoredPosition = hiddenPosition;
            canvasGroup.alpha = 0;
        }

        public void ShowHint(string message) => Show(message);

        public void ShowWaveNotification(int waveNumber)
        {
            var message = waveNumber <= totalWavesCount
                ? $"Началась волна {waveNumber} из {totalWavesCount}"
                : $"Началась волна {waveNumber}";
            Show(message);
        }

        public void HideHint()
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            panelRect.anchoredPosition = hiddenPosition;
            canvasGroup.alpha = 0;
        }

        private void Show(string message)
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            textElement.text = message;
            activeCoroutine = StartCoroutine(NotificationCycle());
        }

        private IEnumerator NotificationCycle()
        {
            yield return MovePanel(hiddenPosition, visiblePosition, slideInDuration, entranceCurve, true);

            float timer = 0;
            while (timer < displayDuration)
            {
                if (Time.timeScale > 0) timer += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return MovePanel(visiblePosition, hiddenPosition, slideOutDuration, exitCurve, false);
            activeCoroutine = null;
        }

        private IEnumerator MovePanel(Vector2 start, Vector2 end, float duration, AnimationCurve curve, bool fadeIn)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = elapsed / duration;
                var curveValue = curve.Evaluate(t);

                panelRect.anchoredPosition = Vector2.LerpUnclamped(start, end, curveValue);
                canvasGroup.alpha = fadeIn ? Mathf.Lerp(0, 1, t * 2) : Mathf.Lerp(1, 0, t);
                yield return null;
            }
            panelRect.anchoredPosition = end;
            canvasGroup.alpha = fadeIn ? 1 : 0;
        }
    }
}