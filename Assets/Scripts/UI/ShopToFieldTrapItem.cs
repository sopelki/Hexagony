using Logic.Trap;
using Field;
using HexagonScripts;
using Logic.Trap.Logic.Trap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace UI
{
    public class ShopToFieldTrapItem : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI & Scene")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform mapViewport;
        [SerializeField] private Tilemap tilemap;

        [Header("Logic")]
        [SerializeField] private TrapData trapData;

        [Header("Ghost")]
        [SerializeField] private float ghostAlpha = 0.7f;

        private TrapSystem trapSystem;
        private Field.Field field;

        private GameObject ghost;
        private Image ghostImage;

        public void Construct(TrapSystem trapSystem, Field.Field field)
        {
            this.trapSystem = trapSystem;
            this.field = field;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (trapSystem == null) return;

            var prefabRenderer = trapData.viewPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabRenderer == null) return;

            ghost = new GameObject("TrapGhost",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            ghost.transform.SetParent(canvas.transform, false);

            ghostImage = ghost.GetComponent<Image>();
            ghostImage.sprite = prefabRenderer.sprite;
            ghostImage.color = new Color(1, 1, 1, ghostAlpha);
            ghostImage.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghost == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);

            ghost.GetComponent<RectTransform>().localPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // if (ghost != null)
            //     Destroy(ghost);
            //
            // var cam = Camera.main;
            // if (!cam) return;
            //
            // var world = cam.ScreenToWorldPoint(eventData.position);
            // var cell = fieldTilemap.WorldToCell(world);
            //
            // var axial = HexagonMath.OffsetToAxial(cell.x, cell.y);
            //
            // var hex = field.GetHex(axial);
            // if (hex == null)
            //     return;
            //
            // if (!field.IsWalkable(hex))
            //     return;
            //
            // trapSystem.TryPlaceTrap(trapData, axial);
            
            if (ghost != null)
            {
                Destroy(ghost);
                ghost = null;
            }

            if (trapSystem == null || field == null)
                return;

            var cam = Camera.main;
            if (!cam || mapViewport == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapViewport,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var local))
                return;

            // переводим в нормализованные координаты viewport
            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;

            var zDist = Mathf.Abs(cam.transform.position.z - tilemap.transform.position.z);

            var world = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));

            var cell = tilemap.WorldToCell(world);

            Debug.Log("Offset cell: " + cell);

            var axial = HexagonScripts.HexagonMath.OffsetToAxial(cell.x, cell.y);

            Debug.Log("Axial: " + axial);

            var hex = field.GetHex(axial);
            if (hex == null)
                return;

            if (!field.IsWalkable(hex))
                return;

            trapSystem.TryPlaceTrap(trapData, axial);
        

            
            // if (ghost != null)
            //     Destroy(ghost);
            //
            // if (trapSystem == null || field == null)
            // {
            //     Debug.LogError("TrapSystem or Field is NULL");
            //     return;
            // }
            //
            // var cam = Camera.main;
            // if (!cam)
            // {
            //     Debug.LogError("Camera is NULL");
            //     return;
            // }
            //
            // var world = cam.ScreenToWorldPoint(eventData.position);
            // var cell = fieldTilemap.WorldToCell(world);
            //
            // Debug.Log("Offset cell: " + cell);
            //
            // // 🔥 ВАЖНО — перевести в axial
            // var axial = HexagonScripts.HexagonMath.OffsetToAxial(cell.x, cell.y);
            //
            // Debug.Log("Trying to place trap at axial: " + axial);
            //
            // var hex = field.GetHex(axial);
            //
            // Debug.Log("Hex exists? " + (hex != null));
            //
            // if (hex == null)
            //     return;
            //
            // Debug.Log("Hex type: " + hex.type);
            //
            // if (!field.IsWalkable(hex))
            // {
            //     Debug.Log("Hex is NOT walkable");
            //     return;
            // }
            //
            // var placed = trapSystem.TryPlaceTrap(trapData, axial);
            //
            // Debug.Log("Trap placed? " + placed);

        }
    }
}