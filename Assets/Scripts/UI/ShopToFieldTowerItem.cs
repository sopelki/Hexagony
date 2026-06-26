using System.Collections;
using Logic.Tower;
using Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using View;

namespace UI
{
    public class ShopToFieldTowerItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI & Scene")]
        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private RectTransform mapViewport;
        [SerializeField]
        private Tilemap fieldTilemap;
        [SerializeField]
        private TileBase slotTile;

        [Header("Ghost Settings")]
        [SerializeField]
        private Vector2 ghostOffset = Vector2.zero;
        [SerializeField]
        private bool enableSnapping = true;
        [SerializeField]
        private int searchRadius = 3;
        [SerializeField]
        private float detectionDistance = 50f;

        [Header("Ghost Colors")]
        [SerializeField]
        private Color ghostValidColor = new(1f, 1f, 1f, 0.7f);
        [SerializeField]
        private Color ghostInvalidColor = new(1f, 0.6f, 0.6f, 0.7f);

        [Header("Animation & Speeds")]
        [SerializeField]
        private float startScaleMultiplier = 0.75f;
        [SerializeField]
        private float scaleSpeed = 15f;
        [SerializeField]
        private float colorLerpSpeed = 15f;
        [SerializeField]
        private float snapSpeed = 20f;
        [SerializeField]
        private float unSnapSpeed = 15f;

        [Header("Logic")]
        [SerializeField]
        private TowerData towerData;

        [Header("Slot Feedback")]
        [SerializeField]
        private Image iconImage;
        [SerializeField]
        private float fadeDuration = 0.1f;

        private Vector2 currentGhostPosition;
        private Vector3Int currentHoveredSlot;
        private float currentScale;
        private Coroutine fadeCoroutine;
        private GameObject ghost;
        private Image ghostImage;
        private RectTransform ghostRect;

        private CanvasGroup iconCanvasGroup;

        private bool isDragging;

        private bool isSnapping;

        private TowerModel lastPreviewedModel;
        private TowerView lastPreviewedView;
        private Color targetColor;

        private Vector2 targetGhostPosition;
        private float targetScale;

        private TowerSystem towerSystem;
        private bool wasSnapping;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (iconCanvasGroup != null)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

                iconCanvasGroup.alpha = 0f;
            }

            if (TryGetComponent<TooltipTrigger>(out var trigger)) trigger.StopDisplay();

            if (towerSystem == null) return;

            isDragging = true;
            GlobalCursorManager.Instance.SetHold();

            var prefabRenderer = towerData.viewPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabRenderer == null) return;

            CreateGhost(prefabRenderer);

            currentScale = startScaleMultiplier;
            targetScale = startScaleMultiplier;
            targetColor = ghostValidColor;
            isSnapping = false;
            wasSnapping = false;

            ghostRect.localScale = Vector3.one * currentScale;

            UpdateGhostPosition(eventData);
            currentGhostPosition = targetGhostPosition;
            ghostRect.localPosition = currentGhostPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghostRect != null && isDragging) UpdateGhostPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;
            GlobalCursorManager.Instance.ReleaseHold(eventData);

            CleanupGhost();

            var placed = TryPlaceTower(eventData);

            if (placed)
            {
                lastPreviewedModel = null;
                lastPreviewedView = null;
            }
            else ResetCurrentPreview();

            if (iconCanvasGroup != null)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

                fadeCoroutine = StartCoroutine(FadeInIcon());
            }
        }

        public void Construct(TowerSystem system)
        {
            towerSystem = system;
        }

        private void Awake()
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>();

            var trigger = gameObject.AddComponent<TooltipTrigger>();
            trigger.SetContent(towerData);

            if (iconImage != null)
            {
                iconCanvasGroup = iconImage.GetComponent<CanvasGroup>();
                if (iconCanvasGroup == null) iconCanvasGroup = iconImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnDisable()
        {
            if (isDragging)
            {
                isDragging = false;
                ResetCurrentPreview();
                CleanupGhost();
                GlobalCursorManager.Instance.ReleaseHold(null);

                if (iconCanvasGroup != null) iconCanvasGroup.alpha = 1f;
            }
        }

        private void Update()
        {
            if (!ghostRect) return;

            if (isSnapping)
            {
                currentGhostPosition = Vector2.Lerp(currentGhostPosition, targetGhostPosition,
                    Time.unscaledDeltaTime * snapSpeed);
                wasSnapping = true;
            }
            else if (wasSnapping)
            {
                currentGhostPosition = Vector2.Lerp(currentGhostPosition, targetGhostPosition,
                    Time.unscaledDeltaTime * unSnapSpeed);

                if (Vector2.Distance(currentGhostPosition, targetGhostPosition) < 1f) wasSnapping = false;
            }
            else currentGhostPosition = targetGhostPosition;

            ghostRect.localPosition = currentGhostPosition;

            currentScale = Mathf.Lerp(currentScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
            ghostRect.localScale = Vector3.one * currentScale;

            ghostImage.color = Color.Lerp(ghostImage.color, targetColor, Time.unscaledDeltaTime * colorLerpSpeed);
        }

        private void UpdateLevelPreview(bool isSnapping, Vector3Int slotPos)
        {
            if (isSnapping)
            {
                var existingTower = towerSystem.GetTowerAt(slotPos);

                if (existingTower != lastPreviewedModel)
                {
                    ResetCurrentPreview();

                    if (existingTower != null && existingTower.Data.type == towerData.type)
                    {
                        var view = TowerViewManager.Instance.GetViewAtCell(slotPos);
                        if (view != null)
                        {
                            view.ShowPreviewLevel(existingTower.Level + 1);
                            lastPreviewedModel = existingTower;
                            lastPreviewedView = view;
                        }
                    }
                }
            }
            else ResetCurrentPreview();
        }

        private void ResetCurrentPreview()
        {
            if (lastPreviewedView != null && lastPreviewedModel != null)
                lastPreviewedView.ResetPreview(lastPreviewedModel.Level);
            lastPreviewedModel = null;
            lastPreviewedView = null;
        }

        private void CleanupGhost()
        {
            if (ghost != null) Destroy(ghost);
        }

        private bool TryPlaceTower(PointerEventData eventData)
        {
            var cam = Camera.main;
            if (cam == null || mapViewport == null || fieldTilemap == null) return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mapViewport, eventData.position,
                    eventData.pressEventCamera, out var local)) return false;

            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;
            if (u < 0f || u > 1f || v < 0f || v > 1f) return false;

            var zDist = Mathf.Abs(cam.transform.position.z - fieldTilemap.transform.position.z);
            var worldPos = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));
            var cellPos = fieldTilemap.WorldToCell(worldPos);

            if (!TryFindValidSlot(eventData, cellPos, out var closestSlotPos)) return false;

            var spawnPos = fieldTilemap.GetCellCenterWorld(closestSlotPos);
            spawnPos.z = fieldTilemap.transform.position.z;

            return towerSystem.TryPlaceTower(towerData, closestSlotPos, spawnPos);
        }

        private IEnumerator FadeInIcon()
        {
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                iconCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            iconCanvasGroup.alpha = 1f;
            fadeCoroutine = null;
        }

        private void CreateGhost(SpriteRenderer prefabRenderer)
        {
            ghost = new GameObject(name + "_ghost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            ghost.transform.SetParent(canvas.transform, false);
            ghostRect = ghost.GetComponent<RectTransform>();
            ghostImage = ghost.GetComponent<Image>();

            ghostImage.sprite = prefabRenderer.sprite;
            ghostImage.preserveAspect = true;
            ghostImage.raycastTarget = false;
            ghostImage.color = ghostValidColor;

            var sprite = prefabRenderer.sprite;
            ghostRect.pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);

            if (Camera.main != null)
            {
                var pixelsPerUnit = Screen.height / (Camera.main.orthographicSize * 2f);
                var spriteSize = new Vector2(sprite.rect.width / sprite.pixelsPerUnit,
                    sprite.rect.height / sprite.pixelsPerUnit);

                var prefabScale = prefabRenderer.transform.localScale;
                spriteSize.x *= prefabScale.x;
                spriteSize.y *= prefabScale.y;

                ghostRect.sizeDelta = spriteSize * pixelsPerUnit / canvas.scaleFactor;
            }

            ghostRect.anchorMin = ghostRect.anchorMax = new Vector2(0.5f, 0.5f);
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, eventData.position,
                eventData.pressEventCamera, out var localPoint);

            var basePosition = localPoint + ghostOffset;

            if (!towerSystem.CanAffordTower(towerData))
            {
                targetGhostPosition = basePosition;
                isSnapping = false;
            }
            else if (enableSnapping && TryGetSnapPosition(eventData, basePosition, out var snapPosition))
            {
                targetGhostPosition = snapPosition;
                isSnapping = true;
            }
            else
            {
                targetGhostPosition = basePosition;
                isSnapping = false;
            }

            CheckPlacementValidity(eventData);
            UpdateLevelPreview(isSnapping, currentHoveredSlot);
        }

        private bool TryGetSnapPosition(PointerEventData eventData, Vector2 basePosition, out Vector2 snapPosition)
        {
            snapPosition = basePosition;
            var cam = Camera.main;
            if (!cam || !mapViewport || !fieldTilemap) return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mapViewport, eventData.position,
                    eventData.pressEventCamera, out var local)) return false;

            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;

            var zDist = Mathf.Abs(cam.transform.position.z - fieldTilemap.transform.position.z);
            var worldPos = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));
            var cellPos = fieldTilemap.WorldToCell(worldPos);

            if (!TryFindValidSlot(eventData, cellPos, out var slotPos)) return false;
            if (towerSystem.IsCellOccupied(slotPos))
            {
                if (!towerSystem.CanPlaceTower(towerData, slotPos)) return false;
            }

            if (!TryGetSlotCanvasPosition(slotPos, eventData, out var slotCanvasLocal)) return false;

            snapPosition = slotCanvasLocal + ghostOffset;
            return true;
        }

        private void CheckPlacementValidity(PointerEventData eventData)
        {
            var cam = Camera.main;
            if (!cam || !mapViewport || !fieldTilemap) return;

            if (!towerSystem.CanAffordTower(towerData))
            {
                targetScale = startScaleMultiplier;
                targetColor = ghostInvalidColor;
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mapViewport, eventData.position,
                    eventData.pressEventCamera, out var local))
            {
                targetScale = startScaleMultiplier;
                targetColor = ghostValidColor;
                return;
            }

            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;
            var zDist = Mathf.Abs(cam.transform.position.z - fieldTilemap.transform.position.z);
            var worldPos = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));
            var cellPos = fieldTilemap.WorldToCell(worldPos);

            if (TryFindValidSlot(eventData, cellPos, out var slotPos))
            {
                currentHoveredSlot = slotPos;
                var isValid = towerSystem.CanPlaceTower(towerData, slotPos);

                var existingTower = towerSystem.GetTowerAt(slotPos);
                var isUpgrade = existingTower != null && existingTower.Data.type == towerData.type;

                targetScale = isValid ? 1f : startScaleMultiplier;

                if (isValid && isUpgrade)
                    targetColor = new Color(ghostValidColor.r, ghostValidColor.g, ghostValidColor.b, 0f);
                else targetColor = isValid ? ghostValidColor : ghostInvalidColor;
            }
            else
            {
                currentHoveredSlot = new Vector3Int(-999, -999, -999);
                targetScale = startScaleMultiplier;
                targetColor = ghostValidColor;
            }
        }

        private bool TryFindValidSlot(PointerEventData eventData, Vector3Int centerCell, out Vector3Int validSlot)
        {
            validSlot = Vector3Int.zero;
            if (!FindNearestSlotTile(centerCell, searchRadius, out var nearestSlot)) return false;
            if (!TryGetSlotCanvasPosition(nearestSlot, eventData, out var slotCanvasPos)) return false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, eventData.position,
                eventData.pressEventCamera, out var cursorCanvasPos);

            var cursorPosition = cursorCanvasPos + ghostOffset;
            var distance = Vector2.Distance(cursorPosition, slotCanvasPos + ghostOffset);

            if (distance <= detectionDistance)
            {
                validSlot = nearestSlot;
                return true;
            }
            return false;
        }

        private bool TryGetSlotCanvasPosition(Vector3Int slotCell, PointerEventData eventData,
            out Vector2 canvasPosition)
        {
            canvasPosition = Vector2.zero;
            var cam = Camera.main;
            if (!cam || !mapViewport || !fieldTilemap) return false;

            var slotWorldCenter = fieldTilemap.GetCellCenterWorld(slotCell);
            var slotViewport = cam.WorldToViewportPoint(slotWorldCenter);

            var slotLocalInViewport = new Vector2((slotViewport.x - 0.5f) * mapViewport.rect.width,
                (slotViewport.y - 0.5f) * mapViewport.rect.height);

            var slotScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera,
                mapViewport.TransformPoint(slotLocalInViewport));

            return RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform,
                slotScreenPos, eventData.pressEventCamera, out canvasPosition);
        }

        private bool FindNearestSlotTile(Vector3Int centerPos, int slotSearchRadius, out Vector3Int cellPos)
        {
            var nearestPos = Vector3Int.zero;
            var nearestDistance = float.MaxValue;
            var found = false;

            for (var x = centerPos.x - slotSearchRadius; x <= centerPos.x + slotSearchRadius; x++)
            {
                for (var y = centerPos.y - slotSearchRadius; y <= centerPos.y + slotSearchRadius; y++)
                {
                    var checkPos = new Vector3Int(x, y, centerPos.z);
                    var tile = fieldTilemap.GetTile(checkPos);

                    if (tile == null || slotTile == null || tile.name != slotTile.name) continue;

                    var distance = Vector3Int.Distance(checkPos, centerPos);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPos = checkPos;
                        found = true;
                    }
                }
            }
            cellPos = nearestPos;
            return found;
        }
    }
}