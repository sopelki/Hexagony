using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using View;

namespace Misc
{
    public class ViewportRaycaster : MonoBehaviour, IPointerClickHandler
    {
        [Header("Настройки")]
        [SerializeField]
        private Camera worldCamera;
        [SerializeField]
        private RectTransform viewportRect;

        private MonsterInteractionHandler lastHoveredMonster;
        private Canvas parentCanvas;

        private void Awake()
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            Vector2 mousePos;
            if (Mouse.current != null)
                mousePos = Mouse.current.position.ReadValue();
            else
                mousePos = Input.mousePosition;

            UpdateMonsterHover(mousePos);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var monster = RaycastAtPosition(eventData.position);
            if (monster)
                monster.OnPointerClick(eventData);
        }

        private void UpdateMonsterHover(Vector2 screenPosition)
        {
            if (!worldCamera || !viewportRect) return;

            var currentMonster = RaycastAtPosition(screenPosition);

            if (currentMonster != lastHoveredMonster)
            {
                if (lastHoveredMonster) lastHoveredMonster.OnPointerExit(null);
                if (currentMonster) currentMonster.OnPointerEnter(null);

                lastHoveredMonster = currentMonster;
            }

            if (lastHoveredMonster != null)
            {
                if (lastHoveredMonster.IsModelDead() || !lastHoveredMonster.gameObject.activeInHierarchy)
                {
                    lastHoveredMonster.OnPointerExit(null);
                    lastHoveredMonster = null;
                }
            }
        }

        private MonsterInteractionHandler RaycastAtPosition(Vector2 screenPosition)
        {
            var uiCam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? parentCanvas.worldCamera
                : null;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewportRect, screenPosition, uiCam,
                    out var localPoint))
                return null;

            var rect = viewportRect.rect;
            var normX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            var normY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

            if (normX < 0 || normX > 1 || normY < 0 || normY > 1)
                return null;

            var ray = worldCamera.ViewportPointToRay(new Vector3(normX, normY, 0));
            var hit = Physics2D.GetRayIntersection(ray);

            return hit.collider != null ? hit.collider.GetComponent<MonsterInteractionHandler>() : null;
        }
    }
}