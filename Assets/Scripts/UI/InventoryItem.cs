using System;
using Logic.Castle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Image))]
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public event Action OnDropped;

        [Header("Data")]
        [SerializeField]
        private BuildingData buildingData;

        [Header("Visual Settings")]
        [SerializeField]
        private float scaleSpeed = 30f;
        [SerializeField]
        private float colorLerpSpeed = 15f;
        [SerializeField]
        [Range(0f, 1f)]
        private float draggingAlpha = 0.8f;

        private Vector3 originalScale;
        private Vector3 targetScale;
        private CanvasGroup canvasGroup;
        private Image itemImage;
        private CastleDragHandler dragHandler;

        private Color originalColor;
        private Color normalDraggingColor;
        private Color invalidColor;
        private Color targetColor;

        public BuildingData BuildingData => buildingData;
        public Transform OriginalParent { get; private set; }
        public bool IsFromShop { get; private set; }

        private void Awake()
        {
            dragHandler = GetComponent<CastleDragHandler>();
            canvasGroup = GetComponent<CanvasGroup>();
            itemImage = GetComponent<Image>();

            originalScale = transform.localScale;
            targetScale = originalScale;
            originalColor = itemImage.color;
            normalDraggingColor = new Color(originalColor.r, originalColor.g, originalColor.b, draggingAlpha);
            invalidColor = new Color(1f, 0.6f, 0.6f, draggingAlpha);
            targetColor = originalColor;
        }

        public void SetDraggingScale(float multiplier) => targetScale = originalScale * multiplier;
        public void SetValidationState(bool isValid) => targetColor = isValid ? normalDraggingColor : invalidColor;

        public void OnBeginDrag(PointerEventData eventData)
        {
            GetComponent<TooltipTrigger>()?.StopDisplay();
            canvasGroup.blocksRaycasts = false;
            targetColor = invalidColor;

            if (!IsFromShop)
                CaptureState();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            targetScale = originalScale;
            targetColor = originalColor;
            canvasGroup.blocksRaycasts = true;

            OnDropped?.Invoke();

            if (transform.parent != dragHandler.MainCanvas.transform)
                return;

            if (IsFromShop) 
                Destroy(gameObject);
            else
                ReturnToStart();
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            itemImage.color = Color.Lerp(itemImage.color, targetColor, Time.deltaTime * colorLerpSpeed);
        }

        public void SetData(BuildingData data, bool fromShop)
        {
            buildingData = data;
            IsFromShop = fromShop;
            var trigger = GetComponent<TooltipTrigger>() ?? gameObject.AddComponent<TooltipTrigger>();
            trigger.SetContent(buildingData, true);
        }

        public void Place(Transform slot)
        {
            transform.SetParent(slot);
            dragHandler.ResetPosition();
            IsFromShop = false;
            targetColor = originalColor;
        }

        private void CaptureState()
        {
            OriginalParent = transform.parent;
            transform.SetParent(dragHandler.MainCanvas.transform);
        }

        private void ReturnToStart()
        {
            transform.SetParent(OriginalParent);
            dragHandler?.ResetPosition();
        }
        
        private void OnDestroy()
        {
            OnDropped?.Invoke();
        }
    }
}