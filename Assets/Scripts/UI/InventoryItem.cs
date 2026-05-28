using System;
using Audio;
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
        [Header("Data")]
        [SerializeField]
        private BuildingData buildingData;
        [SerializeField]
        private SoundData soundData;

        [Header("Visual Settings")]
        [SerializeField]
        private float scaleSpeed = 30f;
        [SerializeField]
        private float colorLerpSpeed = 15f;
        [SerializeField]
        [Range(0f, 1f)]
        private float draggingAlpha = 0.8f;
        private CanvasGroup canvasGroup;
        private CastleDragHandler dragHandler;
        private Color invalidColor;
        private Image itemImage;
        private Color normalDraggingColor;

        private Color originalColor;

        private Vector3 originalScale;
        private Color targetColor;
        private Vector3 targetScale;

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

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            itemImage.color = Color.Lerp(itemImage.color, targetColor, Time.deltaTime * colorLerpSpeed);
        }

        private void OnDestroy()
        {
            OnDropped?.Invoke();
        }

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

        public event Action OnDropped;

        public void SetDraggingScale(float multiplier)
        {
            targetScale = originalScale * multiplier;
        }

        public void SetValidationState(bool isValid)
        {
            targetColor = isValid ? normalDraggingColor : invalidColor;
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
            AudioManager.Instance.PlaySfx(soundData.buildingPlaceSound, soundData.buildingPlacementVolume);
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
    }
}