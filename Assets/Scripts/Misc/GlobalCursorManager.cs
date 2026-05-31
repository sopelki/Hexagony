using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UI;

namespace Misc
{
    public class GlobalCursorManager : MonoBehaviour
    {
        public static GlobalCursorManager Instance;

        [Header("Textures")]
        [SerializeField]
        private Texture2D defaultCursor;
        [SerializeField]
        private Texture2D interactCursor;
        [SerializeField]
        private Texture2D holdCursor;

        [Header("Hotspots")]
        [SerializeField]
        private Vector2 defaultHotspot = Vector2.zero;
        [SerializeField]
        private Vector2 interactHotspot = new(10, 0);
        [SerializeField]
        private Vector2 holdHotspot = new(16, 16);

        private bool holdLock;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetDefault();
            }
            else
                Destroy(gameObject);
        }

        public void SetDefault()
        {
            if (holdLock) return;
            Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
        }

        public void SetInteract()
        {
            if (holdLock) return;
            Cursor.SetCursor(interactCursor, interactHotspot, CursorMode.Auto);
        }

        public void SetHold()
        {
            holdLock = true;
            Cursor.SetCursor(holdCursor, holdHotspot, CursorMode.Auto);
        }

        public void ReleaseHold(PointerEventData eventData)
        {
            holdLock = false;

            if (eventData != null && IsPointerOverInteractiveUI(eventData))
                Cursor.SetCursor(interactCursor, interactHotspot, CursorMode.Auto);
            else
                SetDefault();
        }

        private static bool IsPointerOverInteractiveUI(PointerEventData eventData)
        {
            if (EventSystem.current == null) return false;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Any(result => result.gameObject.GetComponentInParent<UICursorTrigger>() != null);
        }
    }
}