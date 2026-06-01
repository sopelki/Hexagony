using System.Collections;
using Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float Delay = 0.5f;
        private Coroutine delayCoroutine;
        private bool isBought;
        private ITooltipProvider provider;

        private void OnDisable()
        {
            StopDisplay();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
                return;

            if (provider == null)
                return;

            delayCoroutine = StartCoroutine(ShowWithDelay());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopDisplay();
        }

        public void SetContent(ITooltipProvider tooltipProvider, bool bought = false)
        {
            provider = tooltipProvider;
            isBought = bought;
        }

        private IEnumerator ShowWithDelay()
        {
            yield return new WaitForSecondsRealtime(Delay);
            TooltipUI.Instance.Show(provider.GetTooltipContent(isBought));
        }

        public void StopDisplay()
        {
            if (delayCoroutine != null)
                StopCoroutine(delayCoroutine);

            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();
        }
    }
}