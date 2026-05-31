using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HexagonScripts;
using Logic.Trap;
using Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
        private float validSizeScale = 1f;

        [Header("Animation & Speeds")]
        [SerializeField]
        private float snapSpeed = 20f;
        [SerializeField]
        private float unSnapSpeed = 15f;
        [SerializeField]
        private Vector2 ghostOffset = Vector2.zero;

        [Header("Slot Feedback")]
        [SerializeField]
        private Image iconImage;
        [SerializeField]
        private float fadeDuration = 0.1f;
        private readonly List<GameObject> highlights = new();
        private Vector2 currentGhostPosition;
        private float currentScale;
        private Coroutine fadeCoroutine;
        private Field.Field field;
        private GameObject ghost;
        private Image ghostImage;
        private RectTransform ghostRect;

        private CanvasGroup iconCanvasGroup;
        private bool isSnapping;
        private bool wasSnapping;
        private Color targetColor;
        private Vector2 targetGhostPosition;
        private TrapSystem trapSystem;
        private bool isDragging;

        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            var trigger = gameObject.AddComponent<TooltipTrigger>();
            trigger.SetContent(trapData);

            if (iconImage != null)
            {
                iconCanvasGroup = iconImage.GetComponent<CanvasGroup>();
                if (iconCanvasGroup == null)
                    iconCanvasGroup = iconImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Update()
        {
            if (!ghost)
                return;
            if (isSnapping)
            {
                currentGhostPosition =
                    Vector2.Lerp(currentGhostPosition, targetGhostPosition, Time.deltaTime * snapSpeed);
                wasSnapping = true;
            }
            else if (wasSnapping)
            {
                currentGhostPosition =
                    Vector2.Lerp(currentGhostPosition, targetGhostPosition, Time.deltaTime * unSnapSpeed);
                if (Vector2.Distance(currentGhostPosition, targetGhostPosition) < 1f)
                    wasSnapping = false;
            }
            else
                currentGhostPosition = targetGhostPosition;
            ghostRect.localPosition = currentGhostPosition;
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleSpeed);
            ghostRect.localScale = Vector3.one * currentScale;
            ghostImage.color = Color.Lerp(ghostImage.color, targetColor, Time.deltaTime * colorLerpSpeed);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (trapSystem == null)
                return;

            GlobalCursorManager.Instance.SetHold();
            isDragging = true;

            if (iconCanvasGroup != null)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);

                iconCanvasGroup.alpha = 0f;
            }

            ClearHighlights();

            var prefabRenderer = trapData.viewPrefab.GetComponentInChildren<SpriteRenderer>();

            ghost = new GameObject("TrapGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ghost.transform.SetParent(canvas.transform, false);
            ghostRect = ghost.GetComponent<RectTransform>();
            ghostImage = ghost.GetComponent<Image>();
            ghostImage.sprite = prefabRenderer.sprite;
            ghostImage.preserveAspect = true;
            ghostImage.raycastTarget = false;

            var sprite = prefabRenderer.sprite;
            ghostRect.pivot = new Vector2(0.5f, 0.5f);

            if (Camera.main != null)
            {
                var pixelsPerUnit = Screen.height / (Camera.main.orthographicSize * 2f);
                var spriteSize = new Vector2(
                    sprite.rect.width / sprite.pixelsPerUnit,
                    sprite.rect.height / sprite.pixelsPerUnit
                );

                var prefabScale = prefabRenderer.transform.localScale;
                spriteSize.x *= prefabScale.x;
                spriteSize.y *= prefabScale.y;

                ghostRect.sizeDelta = spriteSize * pixelsPerUnit / canvas.scaleFactor;
            }

            ghostRect.anchorMin = ghostRect.anchorMax = new Vector2(0.5f, 0.5f);

            currentScale = startScaleMultiplier;
            targetScale = startScaleMultiplier;
            targetColor = ghostValidColor;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);

            currentGhostPosition = localPoint;
            targetGhostPosition = localPoint;
            ghostRect.localPosition = currentGhostPosition;

            for (var i = 0; i < 3; i++)
            {
                var h = Instantiate(highlightPrefab);
                h.SetActive(false);
                highlights.Add(h);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghostRect == null || !isDragging)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);
            ghostRect.localPosition = localPoint;

            UpdatePlacementFeedback(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;

            GlobalCursorManager.Instance.ReleaseHold(eventData);
            isDragging = false;

            if (TryGetCellUnderMouse(eventData, out var cellPos))
            {
                var axial = HexagonMath.OffsetToAxial(cellPos.x, cellPos.y);
                trapSystem.TryPlaceTrap(trapData, axial);
            }

            CleanupDraggingUI();

            if (iconCanvasGroup != null)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);

                fadeCoroutine = StartCoroutine(FadeInIcon());
            }
        }

        private void OnDisable()
        {
            if (isDragging)
            {
                GlobalCursorManager.Instance.ReleaseHold(null);
                isDragging = false;
                CleanupDraggingUI();
                if (iconCanvasGroup != null)
                    iconCanvasGroup.alpha = 1f;
            }
        }

        private void CleanupDraggingUI()
        {
            if (ghost != null)
                Destroy(ghost);

            ClearHighlights();
        }

        public void Construct(TrapSystem trapSystem, Field.Field field)
        {
            this.trapSystem = trapSystem;
            this.field = field;
        }


        private void UpdatePlacementFeedback(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                eventData.position,
                eventData.pressEventCamera,
                out var mouseLocalPoint);

            targetGhostPosition = mouseLocalPoint + ghostOffset;
            isSnapping = false;

            if (TryGetCellUnderMouse(eventData, out var cellPos))
            {
                var axial = HexagonMath.OffsetToAxial(cellPos.x, cellPos.y);
                var isValid = trapSystem.CanPlaceTrap(trapData, axial);

                targetColor = isValid ? ghostValidColor : ghostInvalidColor;
                targetScale = isValid ? validSizeScale : startScaleMultiplier;

                var hexObj = field.GetHex(axial);

                if (hexObj != null && isValid &&
                    TryGetSlotCanvasPosition(hexObj.offset, eventData, out var snappedCanvasPos))
                {
                    targetGhostPosition = snappedCanvasPos + ghostOffset;
                    isSnapping = true;
                }
                else
                {
                    targetGhostPosition = mouseLocalPoint + ghostOffset;
                    isSnapping = false;
                }

                UpdateHighlights(axial, isValid);
            }
            else
            {
                targetColor = ghostInvalidColor;
                targetScale = startScaleMultiplier;
                highlights.ForEach(h => h.SetActive(false));
            }
        }

        private void UpdateHighlights(Vector2Int axial, bool isValid)
        {
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
                else
                    highlights[i].SetActive(false);
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

        private IEnumerator FadeInIcon()
        {
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                iconCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            iconCanvasGroup.alpha = 1f;
            fadeCoroutine = null;
        }

        private void ClearHighlights()
        {
            foreach (var h in highlights.Where(h => h != null))
                Destroy(h);

            highlights.Clear();
        }

        private bool TryGetSlotCanvasPosition(Vector3Int slotCell, PointerEventData eventData,
            out Vector2 canvasPosition)
        {
            canvasPosition = Vector2.zero;
            var cam = Camera.main;
            if (!cam || !mapViewport || !fieldTilemap)
                return false;

            var slotWorldCenter = fieldTilemap.GetCellCenterWorld(slotCell);
            var slotViewport = cam.WorldToViewportPoint(slotWorldCenter);

            var slotLocalInViewport = new Vector2(
                (slotViewport.x - 0.5f) * mapViewport.rect.width,
                (slotViewport.y - 0.5f) * mapViewport.rect.height
            );

            var slotScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera,
                mapViewport.TransformPoint(slotLocalInViewport));

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                slotScreenPos,
                eventData.pressEventCamera,
                out canvasPosition);
        }
    }
}