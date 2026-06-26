using TMPro;
using UnityEngine;

namespace MenuScripts
{
    public class MainMenuHighScore : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI splashText;

        [Header("Animation Settings")]
        [SerializeField]
        private float pulseSpeed = 5f;
        [SerializeField]
        private float pulseAmount = 0.1f;
        [SerializeField]
        private Color splashColor = new(252, 164, 25);

        private Vector3 originalScale;

        [ContextMenu("Clear High Score")]
        public void ClearHighScore()
        {
            PlayerPrefs.DeleteKey("MaxWaveReached");
            PlayerPrefs.Save();

            if (splashText != null)
                splashText.gameObject.SetActive(false);

            Debug.Log("High Score Cleared!");
        }

        private void Start()
        {
            var highScore = PlayerPrefs.GetInt("MaxWaveReached", 0);

            if (highScore > 0)
            {
                splashText.text = $"Рекорд волн: {highScore}";
                splashText.color = splashColor;
                splashText.gameObject.SetActive(true);
                originalScale = splashText.transform.localScale;
            }
            else
                splashText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (splashText.gameObject.activeSelf)
            {
                var scaleOffset = Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
                splashText.transform.localScale = originalScale * (1f + scaleOffset);
            }
        }
    }
}