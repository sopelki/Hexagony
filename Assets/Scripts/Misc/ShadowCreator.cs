using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Misc
{
    public class ShadowCreator : MonoBehaviour
    {
        public Sprite shadowSprite;
        public bool flipX;
        public bool flipY;
        public Vector2 offset = new(0f, 0f);
        public Vector2 scale = new(1f, 1f);
        [Range(0, 1)]
        public float alpha = 1f;

#if UNITY_EDITOR
        public void CreateShadow()
        {
            const string shadowName = "_Shadow";

            if (transform.Find(shadowName) != null)
            {
                RemoveShadow();
                Debug.Log("Existing shadow was removed.");
            }

            var parentRenderer = GetComponent<SpriteRenderer>();
            if (parentRenderer == null)
            {
                Debug.LogWarning("No SpriteRenderer found.");
                return;
            }

            var shadow = new GameObject(shadowName);
            shadow.transform.SetParent(transform, false);

            shadow.transform.localPosition = offset;
            shadow.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var spriteRenderer = shadow.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = shadowSprite;
            spriteRenderer.color = new Color(0, 0, 0, alpha);

            spriteRenderer.flipX = flipX;
            spriteRenderer.flipY = flipY;

            spriteRenderer.sortingLayerID = parentRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = parentRenderer.sortingOrder - 1;

            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            EditorUtility.SetDirty(gameObject);
        }

        public void RemoveShadow()
        {
            var existing = transform.Find("_Shadow");
            if (existing != null)
                DestroyImmediate(existing.gameObject);

            EditorUtility.SetDirty(gameObject);
        }
#endif
    }
}