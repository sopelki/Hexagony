using Logic.Castle;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))] 
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [Header("Data")]
        [SerializeField] private BuildingData buildingData;

        private Vector3 originalScale;
        private Vector3 targetScale;
        private CanvasGroup canvasGroup;
        
        [SerializeField] private float scaleSpeed = 30f;
        private CastleDragHandler dragHandler;

        public BuildingData BuildingData => buildingData;
        public Transform OriginalParent { get; private set; }
        public bool IsFromShop { get; private set; }

        private void Awake()
        {
            dragHandler = GetComponent<CastleDragHandler>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            originalScale = transform.localScale; 
            targetScale = originalScale;
        }

        public void SetDraggingScale(float multiplier) => targetScale = originalScale * multiplier;

        public void OnBeginDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = false;

            if (IsFromShop) return;
            CaptureState();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            targetScale = originalScale;
            canvasGroup.blocksRaycasts = true;

            if (transform.parent != dragHandler.MainCanvas.transform)
                return;

            if (IsFromShop)
                Destroy(gameObject);
            else
                ReturnToStart();
        }

        private void Update()
        {
            if (transform.localScale != targetScale)
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }

        public void SetData(BuildingData data, bool fromShop) { buildingData = data; IsFromShop = fromShop; }
        public void Place(Transform slot) { transform.SetParent(slot); dragHandler.ResetPosition(); IsFromShop = false; }
        private void CaptureState() { OriginalParent = transform.parent; transform.SetParent(dragHandler.MainCanvas.transform); }
        private void ReturnToStart() { transform.SetParent(OriginalParent); }
    }
}