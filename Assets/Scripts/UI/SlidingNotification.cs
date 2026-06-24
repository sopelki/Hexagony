using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using TMPro;
using UnityEngine;

namespace UI
{
    public class SlidingNotificationUI : MonoBehaviour
    {
        private struct NotificationRequest
        {
            public string Message;
            public AudioClip Clip;
            public float Volume;
        }

        [Header("References")]
        [SerializeField]
        private RectTransform panelRect;
        [SerializeField]
        private TextMeshProUGUI textElement;
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private SoundData soundData;

        [Header("Position Settings")]
        [SerializeField]
        private Vector2 hiddenPosition = new(0, 250);
        [SerializeField]
        private Vector2 visiblePosition = new(0, -100);

        [Header("Animation Curves")]
        [SerializeField]
        private AnimationCurve entranceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField]
        private AnimationCurve exitCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Durations")]
        [SerializeField]
        private float slideInDuration = 0.6f;
        [SerializeField]
        private float slideOutDuration = 0.3f;
        [SerializeField]
        private float displayDuration = 3f;
        [SerializeField]
        private float queueDelay = 0.3f;

        private readonly Queue<NotificationRequest> queue = new();
        private bool isProcessing;
        private int totalWavesCount;

        public void Initialize(int totalWaves = 0)
        {
            totalWavesCount = totalWaves;
            isProcessing = false;
            queue.Clear();

            panelRect.anchoredPosition = hiddenPosition;
            canvasGroup.alpha = 0;
        }

        public void ShowHint(string message)
        {
            if (soundData == null)
                return;

            Enqueue(message, soundData.notificationSound, soundData.notificationVolume);
        }

        public void ShowWaveNotification(int waveNumber)
        {
            if (soundData == null)
                return;

            var message = waveNumber <= totalWavesCount
                ? $"Началась волна</color>\n<color=#A10009>{waveNumber} из {totalWavesCount}"
                : $"Началась волна\n<color=#A10009>{waveNumber}";

            Enqueue(message, soundData.waveNotificationSound, soundData.waveNotificationVolume);
        }

        public void HideHint()
        {
            StopAllCoroutines();
            queue.Clear();
            isProcessing = false;
            panelRect.anchoredPosition = hiddenPosition;
            canvasGroup.alpha = 0;
        }

        private void Enqueue(string message, AudioClip clip, float volume)
        {
            if (queue.Any(r => r.Message == message))
                return;

            queue.Enqueue(new NotificationRequest
            {
                Message = message,
                Clip = clip,
                Volume = volume
            });

            if (!isProcessing)
                StartCoroutine(ProcessQueueRoutine());
        }

        private IEnumerator ProcessQueueRoutine()
        {
            isProcessing = true;

            while (queue.Count > 0)
            {
                var request = queue.Dequeue();
                textElement.text = request.Message;

                if (request.Clip)
                    AudioManager.Instance.PlaySfx(request.Clip, request.Volume);
                
                yield return MovePanel(hiddenPosition, visiblePosition, slideInDuration, entranceCurve, true);

                float timer = 0;
                while (timer < displayDuration)
                {
                    if (Time.timeScale > 0)
                        timer += Time.unscaledDeltaTime;
                    yield return null;
                }

                yield return MovePanel(visiblePosition, hiddenPosition, slideOutDuration, exitCurve, false);
                yield return new WaitForSecondsRealtime(queueDelay);
            }

            isProcessing = false;
        }

        private IEnumerator MovePanel(Vector2 start, Vector2 end, float duration, AnimationCurve curve, bool fadeIn)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
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