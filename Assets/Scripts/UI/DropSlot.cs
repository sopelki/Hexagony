using Logic.Castle;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DropSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Transform itemContainer;
        private CastleSystem castleSystem;

        private const float ScaleSize = 0.85f;

        public void Construct(CastleSystem system) => castleSystem = system;

        public void OnDrop(PointerEventData eventData)
        {
            var draggingItem = eventData.pointerDrag?.GetComponent<InventoryItem>();
            if (draggingItem != null)
                draggingItem.SetDraggingScale(1.0f);
            else
                return;

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
            if (eventData.pointerDrag != null)
            {
                var draggingItem = eventData.pointerDrag.GetComponent<InventoryItem>();
                if (draggingItem != null)
                {
                    var existingItem = GetStoredItem();

                    if (!draggingItem.IsFromShop || draggingItem.IsFromShop && existingItem == null)
                        draggingItem.SetDraggingScale(ScaleSize);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var draggingItem = eventData.pointerDrag.GetComponent<InventoryItem>();
                if (draggingItem != null)
                    draggingItem.SetDraggingScale(1.0f);
            }
        }

        private InventoryItem GetStoredItem() => itemContainer.GetComponentInChildren<InventoryItem>();
    }
}