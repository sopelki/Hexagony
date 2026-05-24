using Logic.Castle;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DropSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
    {
        [Header("References")]
        [SerializeField]
        private Transform itemContainer;

        [Header("Visual Settings")]
        [SerializeField]
        [Range(0.5f, 1f)]
        private float hoverScale = 0.85f;

        private static CastleSystem castleSystem;

        private InventoryItem currentOverlappingItem;

        public void Construct(CastleSystem system) => castleSystem = system;

        private void Update()
        {
            if (!currentOverlappingItem)
                return;

            if (!EventSystem.current)
            {
                ResetSlotState();
                return;
            }

            UpdateItemVisualState(currentOverlappingItem);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

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
                {
                    ResetSlotState();
                    return;
                }

                if (castleSystem.TryBuyBuilding(draggingItem.BuildingData))
                    draggingItem.Place(itemContainer);
                else
                    Destroy(draggingItem.gameObject);

                ResetSlotState();
                return;
            }

            if (existingItem != null)
                existingItem.Place(draggingItem.OriginalParent);

            draggingItem.Place(itemContainer);
            ResetSlotState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            var draggingItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (draggingItem == null)
                return;

            currentOverlappingItem = draggingItem;
            UpdateItemVisualState(currentOverlappingItem);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            var draggingItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (draggingItem != null && draggingItem == currentOverlappingItem)
            {
                draggingItem.SetDraggingScale(1.0f);
                draggingItem.SetValidationState(false);
                ResetSlotState();
            }
        }

        private void UpdateItemVisualState(InventoryItem draggingItem)
        {
            var existingItem = GetStoredItem();
            var isValid = CanPlaceItem(draggingItem, existingItem);

            if (isValid)
            {
                draggingItem.SetDraggingScale(hoverScale);
                draggingItem.SetValidationState(true);
            }
            else
            {
                draggingItem.SetDraggingScale(1.0f);
                draggingItem.SetValidationState(false);
            }
        }

        private void ResetSlotState() => currentOverlappingItem = null;

        private static bool CanPlaceItem(InventoryItem draggingItem, InventoryItem existingItem)
        {
            if (draggingItem.IsFromShop)
            {
                var isSlotEmpty = existingItem == null;
                var canAfford = castleSystem.CanAfford(draggingItem.BuildingData.baseCost);
                return isSlotEmpty && canAfford;
            }

            return true;
        }

        private InventoryItem GetStoredItem() => itemContainer.GetComponentInChildren<InventoryItem>();
    }
}