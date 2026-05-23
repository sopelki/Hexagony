using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenuScripts
{
    public class SteppedSlider : Slider
    {
        [SerializeField]
        private float step = 0.1f;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;

            DoStateTransition(SelectionState.Pressed, false);

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                eventData.Use();
                SnapToPointer(eventData);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;
            eventData.Use();
            SnapToPointer(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;
            base.OnPointerUp(eventData);
        }

        private void SnapToPointer(PointerEventData eventData)
        {
            var containerRectTransform = transform as RectTransform;
            if (containerRectTransform == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    containerRectTransform, eventData.position, eventData.pressEventCamera, out var localMouse))
                return;

            var rect = containerRectTransform.rect;
            var unclamped = (localMouse.x - rect.xMin) / rect.width;

            var normalized = Mathf.Clamp01(unclamped);
            var steppedNormalized = Mathf.Round(normalized / step) * step;
            var resValue = Mathf.Lerp(minValue, maxValue, steppedNormalized);

            Set(resValue);
        }
    }
}