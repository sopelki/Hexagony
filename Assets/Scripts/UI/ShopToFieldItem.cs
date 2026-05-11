using Logic.Tower;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace UI
{
    public class ShopToFieldItem : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
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

        [Header("Ghost")]
        [Range(0f, 1f)]
        [SerializeField]
        private float ghostAlpha = 0.7f;
        [SerializeField]
        private Vector2 ghostOffset = Vector2.zero;

        [Header("Snapping")]
        [SerializeField]
        private bool enableSnapping = true;
        [SerializeField]
        private float snapDistance = 50f;
        [SerializeField]
        private float snapSpeed = 20f;

        [Header("Logic")]
        [SerializeField]
        private TowerData towerData;

        [Header("Animation")]
        [SerializeField]
        private float startScaleMultiplier = 0.75f;
        [SerializeField]
        private float scaleSpeed = 15f;
        [SerializeField]
        private float colorLerpSpeed = 15f;

        private TowerSystem towerSystem;

        private GameObject ghost;
        private RectTransform ghostRect;
        private Image ghostImage;

        private float targetScale;
        private float currentScale;

        private Color normalColor;
        private Color invalidColor;
        private Color targetColor;

        private Vector2 targetGhostPosition;
        private Vector2 currentGhostPosition;
        private Vector3Int? currentSnappedCell;

        public void Construct(TowerSystem system)
        {
            towerSystem = system;
        }

        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            normalColor = new Color(1f, 1f, 1f, ghostAlpha);
            invalidColor = new Color(1, 0.78f, 0.78f, ghostAlpha);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (towerSystem == null) return;

            var prefabRenderer = towerData.viewPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabRenderer == null) return;

            CreateGhost(prefabRenderer);

            currentScale = startScaleMultiplier;
            targetScale = startScaleMultiplier;

            targetColor = normalColor;

            ghostRect.localScale = Vector3.one * currentScale;

            UpdateGhostPosition(eventData);
            currentGhostPosition = targetGhostPosition;
            ghostRect.localPosition = currentGhostPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghostRect != null)
                UpdateGhostPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (ghost != null)
                Destroy(ghost);

            currentSnappedCell = null;

            var cam = Camera.main;
            if (cam == null || mapViewport == null || fieldTilemap == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapViewport, eventData.position, eventData.pressEventCamera, out var local))
                return;

            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;
            if (u < 0f || u > 1f || v < 0f || v > 1f)
                return;

            var zDist = Mathf.Abs(cam.transform.position.z - fieldTilemap.transform.position.z);
            var worldPos = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));
            var cellPos = fieldTilemap.WorldToCell(worldPos);

            if (!FindNearestSlotTile(cellPos, 2, out var closestSlotPos))
                return;

            var spawnPos = fieldTilemap.GetCellCenterWorld(closestSlotPos);
            spawnPos.z = fieldTilemap.transform.position.z;

            towerSystem.TryPlaceTower(towerData, closestSlotPos, spawnPos);
        }

        private void Update()
        {
            if (!ghostRect) return;

            currentGhostPosition = Vector2.Lerp(
                currentGhostPosition,
                targetGhostPosition,
                Time.deltaTime * snapSpeed
            );
            ghostRect.localPosition = currentGhostPosition;

            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleSpeed);
            ghostRect.localScale = Vector3.one * currentScale;

            ghostImage.color = Color.Lerp(
                ghostImage.color,
                targetColor,
                Time.deltaTime * colorLerpSpeed
            );
        }

        private void CreateGhost(SpriteRenderer prefabRenderer)
        {
            ghost = new GameObject(name + "_ghost",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            ghost.transform.SetParent(canvas.transform, false);
            ghostRect = ghost.GetComponent<RectTransform>();
            ghostImage = ghost.GetComponent<Image>();

            ghostImage.sprite = prefabRenderer.sprite;
            ghostImage.preserveAspect = true;
            ghostImage.raycastTarget = false;
            ghostImage.color = normalColor;

            var sprite = prefabRenderer.sprite;

            ghostRect.pivot = new Vector2(
                sprite.pivot.x / sprite.rect.width,
                sprite.pivot.y / sprite.rect.height
            );

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
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);

            var basePosition = localPoint + ghostOffset;

            if (enableSnapping && TryGetSnapPosition(eventData, basePosition, out var snapPosition))
                targetGhostPosition = snapPosition;
            else
            {
                targetGhostPosition = basePosition;
                currentSnappedCell = null;
            }

            CheckPlacementValidity(eventData);
        }

        private bool TryGetSnapPosition(PointerEventData eventData, Vector2 basePosition, out Vector2 snapPosition)
        {
            snapPosition = basePosition;

            var cam = Camera.main;
            if (!cam || !mapViewport || !fieldTilemap)
                return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapViewport, eventData.position, eventData.pressEventCamera, out var local))
                return false;

            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;

            var zDist = Mathf.Abs(cam.transform.position.z - fieldTilemap.transform.position.z);
            var worldPos = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));
            var cellPos = fieldTilemap.WorldToCell(worldPos);

            if (!FindNearestSlotTile(cellPos, 2, out var slotPos))
                return false;

            var slotWorldCenter = fieldTilemap.GetCellCenterWorld(slotPos);

            var slotViewport = cam.WorldToViewportPoint(slotWorldCenter);

            var slotLocalInViewport = new Vector2(
                (slotViewport.x - 0.5f) * mapViewport.rect.width,
                (slotViewport.y - 0.5f) * mapViewport.rect.height
            );

            var canvasScaleFactor = canvas.scaleFactor;
            var slotScreenPos = eventData.pressEventCamera 
                ? RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, mapViewport.TransformPoint(slotLocalInViewport))
                : RectTransformUtility.WorldToScreenPoint(null, mapViewport.TransformPoint(slotLocalInViewport));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                slotScreenPos,
                eventData.pressEventCamera,
                out var slotCanvasLocal);

            var distance = Vector2.Distance(basePosition, slotCanvasLocal + ghostOffset);

            if (distance < snapDistance)
            {
                snapPosition = slotCanvasLocal + ghostOffset;
                currentSnappedCell = slotPos;
                return true;
            }

            return false;
        }

        private void CheckPlacementValidity(PointerEventData eventData)
        {
            var cam = Camera.main;
            if (!cam || !mapViewport || !fieldTilemap) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapViewport, eventData.position, eventData.pressEventCamera, out var local))
            {
                targetScale = startScaleMultiplier;
                targetColor = normalColor;
                return;
            }

            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;

            var zDist = Mathf.Abs(cam.transform.position.z - fieldTilemap.transform.position.z);
            var worldPos = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));
            var cellPos = fieldTilemap.WorldToCell(worldPos);

            if (FindNearestSlotTile(cellPos, 2, out var slotPos))
            {
                var occupied = towerSystem.IsCellOccupied(slotPos);

                if (!occupied)
                {
                    targetScale = 1f;
                    targetColor = normalColor;
                }
                else
                {
                    targetScale = startScaleMultiplier;
                    targetColor = invalidColor;
                }
            }
            else
            {
                targetScale = startScaleMultiplier;
                targetColor = normalColor;
            }
        }

        private bool FindNearestSlotTile(Vector3Int centerPos, int searchRadius, out Vector3Int cellPos)
        {
            var nearestPos = Vector3Int.zero;
            var nearestDistance = float.MaxValue;
            var found = false;

            for (var x = centerPos.x - searchRadius; x <= centerPos.x + searchRadius; x++)
            {
                for (var y = centerPos.y - searchRadius; y <= centerPos.y + searchRadius; y++)
                {
                    var checkPos = new Vector3Int(x, y, centerPos.z);
                    var tile = fieldTilemap.GetTile(checkPos);

                    if (tile == null || slotTile == null || tile.name != slotTile.name)
                        continue;

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