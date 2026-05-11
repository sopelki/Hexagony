using Logic.Castle;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DropSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField]
        private Transform itemContainer;

        [Header("Visual Settings")]
        [SerializeField]
        [Range(0.5f, 1f)]
        private float hoverScale = 0.85f;

        private CastleSystem castleSystem;

        public void Construct(CastleSystem system) => castleSystem = system;

        public void OnDrop(PointerEventData eventData)
        {
            var draggingItem = eventData.pointerDrag?.GetComponent<InventoryItem>();
            if (draggingItem == null)
                return;

            draggingItem.SetDraggingScale(1.0f);
            draggingItem.SetValidationState(true);

            var existingItem = GetStoredItem();

            if (draggingItem.IsFromShop)
            {
                if (existingItem != null)
                    return;

                if (castleSystem.TryBuyBuilding(draggingItem.BuildingData))
                    draggingItem.Place(itemContainer);
                else
                    Destroy(draggingItem.gameObject);

                return;
            }

            if (existingItem != null)
                existingItem.Place(draggingItem.OriginalParent);

            draggingItem.Place(itemContainer);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            var draggingItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (draggingItem == null)
                return;

            var existingItem = GetStoredItem();
            var isValid = CanPlaceItem(draggingItem, existingItem);

            if (isValid)
            {
                draggingItem.SetDraggingScale(hoverScale);
                draggingItem.SetValidationState(true);
            }
            else
                draggingItem.SetValidationState(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            var draggingItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (draggingItem != null)
            {
                draggingItem.SetDraggingScale(1.0f);
                draggingItem.SetValidationState(false);
            }
        }

        private static bool CanPlaceItem(InventoryItem draggingItem, InventoryItem existingItem)
        {
            if (draggingItem.IsFromShop)
                return existingItem == null;
            
            return true;
        }

        private InventoryItem GetStoredItem() => itemContainer.GetComponentInChildren<InventoryItem>();
    }
}