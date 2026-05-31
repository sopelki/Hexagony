using Misc;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UICursorTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null) return;

            GlobalCursorManager.Instance.SetInteract();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null) return;

            GlobalCursorManager.Instance.SetDefault();
        }
    }
}