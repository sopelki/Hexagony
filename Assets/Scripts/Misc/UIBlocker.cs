using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Misc
{
    public static class UIBlocker
    {
        public static void BlockAll()
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();

            var allComponents = Object.FindObjectsByType<MonoBehaviour>();

            foreach (var mb in allComponents)
            {
                switch (mb)
                {
                    case TooltipTrigger trigger:
                        trigger.StopDisplay();
                        trigger.enabled = false;
                        break;

                    case IBeginDragHandler or IDragHandler or IEndDragHandler:
                        if (mb is not Slider)
                            mb.enabled = false;
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
                    and not Slider)
                    mb.enabled = true;
            }
        }
    }
}