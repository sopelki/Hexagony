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

        [Header("Ghost (след)")]
        [Range(0f, 1f)]
        [SerializeField]
        private float ghostAlpha = 0.7f;
        [SerializeField]
        private Vector2 ghostOffset = Vector2.zero;

        [Header("Logic Data")]
        [SerializeField]
        private TowerData towerData;

        private TowerSystem towerSystem;
        private GameObject ghost;
        private RectTransform ghostRect;

        [Header("Animation Settings")]
        [SerializeField]
        private float startScaleMultiplier = 0.75f;
        [SerializeField]
        private float scaleSpeed = 15f;

        private float targetScale = 1f;
        private float currentScale = 1f;

        public void Construct(TowerSystem system)
        {
            towerSystem = system;
        }

        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (towerSystem == null) return;

            var prefabRenderer = towerData.viewPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabRenderer == null) return;

            CreateGhost(prefabRenderer);

            currentScale = startScaleMultiplier;
            targetScale = startScaleMultiplier;

            if (ghostRect != null)
                ghostRect.localScale = new Vector3(currentScale, currentScale, 1f);

            UpdateGhostPosition(eventData);
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

            if (towerSystem.TryPlaceTower(towerData, closestSlotPos, spawnPos))
                Debug.Log("Tower placement request sent successfully");
            
            targetScale = startScaleMultiplier;
        }

        private void Update()
        {
            if (ghostRect)
            {
                currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleSpeed);
                ghostRect.localScale = new Vector3(currentScale, currentScale, 1f);
            }
        }


        private void CreateGhost(SpriteRenderer prefabRenderer)
        {
            ghost = new GameObject(name + "_ghost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ghost.transform.SetParent(canvas.transform, false);
            ghostRect = ghost.GetComponent<RectTransform>();

            var ghostImage = ghost.GetComponent<Image>();
            var sprite = prefabRenderer.sprite;
            ghostImage.sprite = sprite;
            ghostImage.preserveAspect = true;
            ghostImage.raycastTarget = false;

            var c = prefabRenderer.color;
            c.a = ghostAlpha;
            ghostImage.color = c;

            var normalizedPivot = new Vector2(
                sprite.pivot.x / sprite.rect.width,
                sprite.pivot.y / sprite.rect.height
            );
            ghostRect.pivot = normalizedPivot;

            if (Camera.main != null)
            {
                var pixelsPerUnit = Screen.height / (Camera.main.orthographicSize * 2f);
                var spriteSizeInUnits = new Vector2(
                    prefabRenderer.sprite.rect.width / prefabRenderer.sprite.pixelsPerUnit,
                    prefabRenderer.sprite.rect.height / prefabRenderer.sprite.pixelsPerUnit
                );

                var prefabScale = prefabRenderer.transform.localScale;
                spriteSizeInUnits.x *= prefabScale.x;
                spriteSizeInUnits.y *= prefabScale.y;
                ghostRect.sizeDelta = spriteSizeInUnits * pixelsPerUnit / canvas.scaleFactor;
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
            ghostRect.localPosition = localPoint + ghostOffset;

            CheckPlacementValidity(eventData);
        }

        private void CheckPlacementValidity(PointerEventData eventData)
        {
            var cam = Camera.main;
            if (cam == null || mapViewport == null || fieldTilemap == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapViewport, eventData.position, eventData.pressEventCamera, out var local))
            {
                targetScale = startScaleMultiplier;
                return;
            }

            var u = local.x / mapViewport.rect.width + 0.5f;
            var v = local.y / mapViewport.rect.height + 0.5f;

            var zDist = Mathf.Abs(cam.transform.position.z - fieldTilemap.transform.position.z);
            var worldPos = cam.ViewportToWorldPoint(new Vector3(u, v, zDist));
            var cellPos = fieldTilemap.WorldToCell(worldPos);

            targetScale = FindNearestSlotTile(cellPos, 2, out _) ? 1f : startScaleMultiplier;
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