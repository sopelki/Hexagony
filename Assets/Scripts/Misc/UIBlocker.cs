using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UI;

namespace Misc
{
    public static class UIBlocker
    {
        public static void BlockAll()
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();

            var allComponents = Object.FindObjectsByType<MonoBehaviour>();

            var currentMousePosition = Vector2.zero;
            if (Mouse.current != null)
                currentMousePosition = Mouse.current.position.ReadValue();

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = currentMousePosition
            };

            foreach (var mb in allComponents)
            {
                switch (mb)
                {
                    case TooltipTrigger trigger:
                        trigger.StopDisplay();
                        trigger.enabled = false;
                        break;

                    case IBeginDragHandler or IDragHandler or IEndDragHandler:
                        if (mb is not UnityEngine.UI.Slider)
                        {
                            if (mb is IEndDragHandler endDragHandler)
                                endDragHandler.OnEndDrag(pointerData);

                            mb.enabled = false;
                        }
                        break;
                }
            }
        }

        public static void UnblockAll()
        {
            var allComponents = Object.FindObjectsByType<MonoBehaviour>();

            foreach (var mb in allComponents)
            {
                if (mb is (TooltipTrigger or IBeginDragHandler or IDragHandler or IEndDragHandler)
                    and not UnityEngine.UI.Slider)
                    mb.enabled = true;
            }
        }
    }
}