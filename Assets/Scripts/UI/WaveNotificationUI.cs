using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    public class WaveNotificationUI : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private float displayDuration = 3f;
        [SerializeField]
        private float fadeDuration = 0.5f;
        [SerializeField]
        private float targetOpacity = 0.75f;
        [SerializeField]
        private SlidingNotificationUI slidingUI;
        
        private Coroutine displayCoroutine;

        private int wavesCount;

        public void Initialize(int wavesCount)
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0;

            this.wavesCount = wavesCount;
        }

        public void ShowWaveNotification(int waveNumber)
        {
            var message = waveNumber <= wavesCount
                ? $"Началась волна {waveNumber} из {wavesCount}"
                : $"Началась волна {waveNumber}";

            slidingUI.Show(message); 
        }
    }
}