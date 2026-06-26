using UnityEngine;
using UnityEngine.Tilemaps;

namespace Field
{
    [RequireComponent(typeof(Camera))]
    public class CameraSetup : MonoBehaviour
    {
        [Header("References")]
        public Tilemap tilemap;

        [Header("Padding")]
        public float padding = -1f;

        private Camera myCamera;

        [ContextMenu("Fit Camera to Grid")]
        public void FitToGrid()
        {
            if (myCamera == null)
                myCamera = GetComponent<Camera>();

            if (tilemap == null)
            {
                Debug.LogWarning("CameraSetup: references are not assigned.");
                return;
            }

            var tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
            if (tilemapRenderer == null)
            {
                Debug.LogWarning("CameraSetup: references are not assigned.");
                return;
            }

            var bounds = tilemapRenderer.bounds;

            var camPos = transform.position;
            camPos.x = bounds.center.x;
            camPos.y = bounds.center.y;
            transform.position = camPos;
            Debug.Log(camPos);
            var halfHeight = bounds.size.y * 0.5f + padding;
            var halfWidth = bounds.size.x * 0.5f + padding;

            var sizeFromWidth = halfWidth / myCamera.aspect;
            myCamera.orthographicSize = Mathf.Max(halfHeight, sizeFromWidth);
        }

        private void Awake()
        {
            myCamera = GetComponent<Camera>();
        }
    }
}