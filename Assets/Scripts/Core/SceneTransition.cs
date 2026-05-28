using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core
{
    public class SceneTransitions : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private float fadeDuration = 0.1f;
        [SerializeField]
        private Color fadeColor = Color.black;

        private CanvasGroup canvasGroup;
        private bool isTransitioning;
        public static SceneTransitions Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateFadeCanvas();
        }

        private void CreateFadeCanvas()
        {
            var g = new GameObject("FadeCanvas");
            g.transform.SetParent(transform);

            var canvas = g.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            canvasGroup = g.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var imgObj = new GameObject("FullImage");
            imgObj.transform.SetParent(g.transform);
            var img = imgObj.AddComponent<Image>();
            img.color = fadeColor;

            var rt = img.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.one;
        }

        public static void LoadScene(string sceneName)
        {
            if (Instance.isTransitioning)
                return;

            Instance.StartCoroutine(Instance.FadeSequence(sceneName));
        }

        private IEnumerator FadeSequence(string sceneName)
        {
            isTransitioning = true;
            canvasGroup.blocksRaycasts = true;

            yield return Fade(1f);

            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (asyncLoad is not { isDone: true })
                yield return null;

            yield return new WaitForSecondsRealtime(0.1f);

            yield return Fade(0f);

            canvasGroup.blocksRaycasts = false;
            isTransitioning = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            var startAlpha = canvasGroup.alpha;
            float elapsed = 0;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }
    }
}