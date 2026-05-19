using UnityEngine;

namespace Misc
{
    public class CanvasSafeArea : MonoBehaviour
    {
        [SerializeField]
        private RectTransform safeAreaTransform;

        [SerializeField]
        private float targetAspect = 16f / 9f;

        private void Start()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (safeAreaTransform == null)
                return;

            var windowAspect = (float)Screen.width / Screen.height;
            var scaleHeight = windowAspect / targetAspect;

            if (scaleHeight < 1.0f)
            {
                safeAreaTransform.anchorMin = new Vector2(0, (1.0f - scaleHeight) / 2.0f);
                safeAreaTransform.anchorMax = new Vector2(1, 1.0f - (1.0f - scaleHeight) / 2.0f);
            }
            else
            {
                var scaleWidth = 1.0f / scaleHeight;
                safeAreaTransform.anchorMin = new Vector2((1.0f - scaleWidth) / 2.0f, 0);
                safeAreaTransform.anchorMax = new Vector2(1.0f - (1.0f - scaleWidth) / 2.0f, 1);
            }

            safeAreaTransform.offsetMin = Vector2.zero;
            safeAreaTransform.offsetMax = Vector2.zero;
        }
    }
}