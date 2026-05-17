using Logic.Trap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace UI
{
    public class ShopToFieldTrapItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI & Scene")]
        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private RectTransform mapViewport;
        [SerializeField]
        private Tilemap fieldTilemap;

        [Header("Ghost Visuals")]
        [SerializeField]
        private float startScaleMultiplier = 0.75f;
        [SerializeField]
        private float scaleSpeed = 15f;
        [SerializeField]
        private float colorLerpSpeed = 15f;

        [Header("Logic")]
        [SerializeField]
        private TrapData trapData;
        [SerializeField]
        private GameObject highlightPrefab;

        [Header("Ghost Colors")]
        [SerializeField]
        private Color ghostValidColor = new(1f, 1f, 1f, 0.7f);
        [SerializeField]
        private Color ghostInvalidColor = new(1f, 0.4f, 0.4f, 0.7f);

        [Header("Highlight Colors")]
        [SerializeField]
        private Color highlightValidColor = new(0f, 1f, 0f, 0.5f);
        [SerializeField]
        private Color highlightInvalidColor = new(1f, 0f, 0f, 0.5f);

        [Header("Animation State")]
        [SerializeField]
        private float targetScale;
        [SerializeField]
        private float validSizeScale = 1.05f;
        private float currentScale;
        private Color targetColor;

        private TrapSystem trapSystem;
        private Field.Field field;
        private GameObject ghost;
        private RectTransform ghostRect;
        private Image ghostImage;
        private readonly List<GameObject> highlights = new();

        public void Construct(TrapSystem trapSystem, Field.Field field)
        {
            this.trapSystem = trapSystem;
            this.field = field;
        }

        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            var trigger = gameObject.AddComponent<TooltipTrigger>();
            trigger.SetContent(trapData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (trapSystem == null)
                return;

            ClearHighlights();

            var prefabRenderer = trapData.viewPrefab.GetComponentInChildren<SpriteRenderer>();

            ghost = new GameObject("TrapGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ghost.transform.SetParent(canvas.transform, false);
            ghostRect = ghost.GetComponent<RectTransform>();
            ghostImage = ghost.GetComponent<Image>();
            ghostImage.sprite = prefabRenderer.sprite;
            ghostImage.preserveAspect = true;
            ghostImage.raycastTarget = false;

            currentScale = startScaleMultiplier;
            targetScale = startScaleMultiplier;
            targetColor = ghostValidColor;

            for (var i = 0; i < 3; i++)
            {
                var h = Instantiate(highlightPrefab);
                h.SetActive(false);
                highlights.Add(h);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghostRect == null)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);
            ghostRect.localPosition = localPoint;

            UpdatePlacementFeedback(eventData);
        }

        private void UpdatePlacementFeedback(PointerEventData eventData)
        {
            if (TryGetCellUnderMouse(eventData, out var cellPos))
            {
                var axial = HexagonScripts.HexagonMath.OffsetToAxial(cellPos.x, cellPos.y);
                var isValid = trapSystem.CanPlaceTrap(trapData, axial);

                targetColor = isValid ? ghostValidColor : ghostInvalidColor;
                targetScale = isValid ? validSizeScale : startScaleMultiplier;

                var hexes = trapSystem.GetTrapOccupiedHexes(axial);
                for (var i = 0; i < highlights.Count; i++)
                {
                    if (i >= hexes.Count)
                    {
                        highlights[i].SetActive(false);
                        continue;
                    }

                    var hexObj = field.GetHex(hexes[i]);
                    if (hexObj != null)
                    {
                        highlights[i].SetActive(true);
                        var worldPos = fieldTilemap.GetCellCenterWorld(hexObj.offset);
                        worldPos.z = -0.5f;
                        highlights[i].transform.position = worldPos;

                        if (highlights[i].TryGetComponent<SpriteRenderer>(out var sr))
                            sr.color = isValid ? highlightValidColor : highlightInvalidColor;
                    }
                    else highlights[i].SetActive(false);
                }
            }
            else
            {
                targetColor = ghostInvalidColor;
                targetScale = startScaleMultiplier;
                highlights.ForEach(h => h.SetActive(false));
            }
        }

        private bool TryGetCellUnderMouse(PointerEventData eventData, out Vector3Int cellPos)
        {
            cellPos = Vector3Int.zero;
            var cam = Camera.main;
            if (!cam || !mapViewport || !fieldTilemap)
                return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mapViewport, eventData.position,
                    eventData.pressEventCamera, out var local))
                return false;

            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;

            if (u < 0f || u > 1f || v < 0f || v > 1f)
                return false;

            var zDist = Mathf.Abs(cam.transform.position.z - fieldTilemap.transform.position.z);
            var worldPos = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));
            cellPos = fieldTilemap.WorldToCell(worldPos);

            return true;
        }

        private void Update()
        {
            if (!ghost)
                return;
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleSpeed);
            ghostRect.localScale = Vector3.one * currentScale;
            ghostImage.color = Color.Lerp(ghostImage.color, targetColor, Time.deltaTime * colorLerpSpeed);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (TryGetCellUnderMouse(eventData, out var cellPos))
            {
                var axial = HexagonScripts.HexagonMath.OffsetToAxial(cellPos.x, cellPos.y);
                trapSystem.TryPlaceTrap(trapData, axial);
            }

            if (ghost != null) Destroy(ghost);
            ClearHighlights();
        }

        private void ClearHighlights()
        {
            foreach (var h in highlights.Where(h => h != null))
                Destroy(h);

            highlights.Clear();
        }
    }
}