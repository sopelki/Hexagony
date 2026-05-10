using Logic.Castle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ShopToCastleItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject inventoryItemPrefab;
        [SerializeField] private BuildingData buildingData;

        private Image sourceImage;

        private void Awake()
        {
            sourceImage = buildingData.viewPrefab.GetComponentInChildren<Image>();
            if (canvas == null) 
                canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log(canvas);
            if (inventoryItemPrefab == null || canvas == null) 
                return;

            var itemGo = Instantiate(inventoryItemPrefab, canvas.transform);
            
            var rt = itemGo.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out var localPoint);
            rt.localPosition = localPoint;

            var item = itemGo.GetComponent<InventoryItem>();
            item.SetData(buildingData, true);

            var itemImage = itemGo.GetComponent<Image>();
            if (sourceImage != null && itemImage != null)
            {
                itemImage.sprite = sourceImage.sprite;
                itemImage.color = sourceImage.color;
                itemImage.preserveAspect = true;
            }

            eventData.pointerDrag = itemGo;
            itemGo.GetComponent<CastleDragHandler>().OnBeginDrag(eventData);
        }
        
        public void OnDrag(PointerEventData eventData) { }
        public void OnEndDrag(PointerEventData eventData) { }
    }
}

