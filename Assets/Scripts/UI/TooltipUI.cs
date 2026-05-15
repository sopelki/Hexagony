using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TooltipUI : MonoBehaviour
    {
        public static TooltipUI Instance;

        [SerializeField]
        private GameObject panel;
        [SerializeField]
        private TextMeshProUGUI titleText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;
        [SerializeField]
        private TextMeshProUGUI priceText;
        [SerializeField]
        private TextMeshProUGUI statsText;

        [Header("Positioning")]
        [SerializeField]
        private int offsetX = 20;
        [SerializeField]
        private int offsetY = -20;

        [Header("Animation Settings")]
        [SerializeField]
        private float fadeInSpeed = 15f;
        [SerializeField]
        private float fadeOutSpeed = 10f;
        [SerializeField]
        private float targetTransparency = 0.7f;
        [SerializeField]
        private Vector3 startScale = new(0.8f, 0.8f, 1f);

        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;

        private bool isVisible;
        private float targetAlpha;
        private Vector3 targetScale;

        private void Awake()
        {
            Instance = this;
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>().rootCanvas;

            canvasGroup.alpha = 0;
            rectTransform.localScale = startScale;
            panel.SetActive(false);
        }

        public void Show(TooltipContent content)
        {
            titleText.text = content.Title;
            descriptionText.text = content.Description;
            statsText.text = content.SpecialInfo;

            if (string.IsNullOrEmpty(content.Cost))
                priceText.gameObject.SetActive(false);
            else
            {
                priceText.gameObject.SetActive(true);
                priceText.text = content.Cost;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            panel.SetActive(true);
            isVisible = true;
            targetAlpha = targetTransparency;
            targetScale = Vector3.one;

            UpdatePosition();
        }

        public void Hide()
        {
            isVisible = false;
            targetAlpha = 0f;
            targetScale = startScale;
        }

        private void Update()
        {
            Animate();

            if (panel.activeSelf)
                UpdatePosition();
        }

        private void Animate()
        {
            var currentSpeed = isVisible ? fadeInSpeed : fadeOutSpeed;

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * currentSpeed);

            rectTransform.localScale =
                Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * currentSpeed);

            if (!isVisible && canvasGroup.alpha < 0.01f)
                panel.SetActive(false);
        }

        private void UpdatePosition()
        {
            var mousePos = Mouse.current.position.ReadValue();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.worldCamera,
                out var localPoint);

            var panelWidth = rectTransform.rect.width;
            var panelHeight = rectTransform.rect.height;

            float finalOffsetX = offsetX;
            float finalOffsetY = offsetY;

            if (mousePos.x + panelWidth + offsetX > Screen.width)
                finalOffsetX = -panelWidth - offsetX;

            if (mousePos.y - panelHeight + offsetY < 0)
                finalOffsetY = panelHeight - offsetY;

            rectTransform.anchoredPosition = localPoint + new Vector2(finalOffsetX, finalOffsetY);
        }
    }
}