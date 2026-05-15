using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
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
        [SerializeField]
        private int offsetX;
        [SerializeField]
        private int offsetY;

        private RectTransform rectTransform;
        private Canvas canvas;

        private void Awake()
        {
            Instance = this;
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>().rootCanvas;
            Hide();
        }

        public void Show(TooltipContent content)
        {
            panel.SetActive(true);

            titleText.text = content.Title;
            descriptionText.text = content.Description;
            priceText.text = content.Cost;
            statsText.text = content.SpecialInfo;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            UpdatePosition();
        }

        public void Hide() => panel.SetActive(false);

        private void Update()
        {
            if (panel.activeSelf)
                UpdatePosition();
        }

        private void UpdatePosition()
        {
            var mousePos = Mouse.current.position.ReadValue();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.worldCamera,
                out var localPoint);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

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