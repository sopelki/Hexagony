using UnityEngine;
using Logic.Projectile;

namespace View
{
    public class ProjectileView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [Header("Animation Settings")]
        [SerializeField]
        private Sprite[] animationFrames;
        [SerializeField]
        private float framesPerSecond = 12f;

        private int currentFrameIndex;
        private float animationTimer;

        private ProjectileModel model;
        private Vector3 lastVisualPosition;

        [Header("Arc Settings")]
        [SerializeField]
        private float maxArcHeight = 2f;
        [SerializeField]
        private float referenceDistance = 8f;
        [SerializeField]
        private float minArcHeightFactor = 0.2f;

        private float currentDynamicHeight;

        public void Initialize(ProjectileModel projectileModel)
        {
            model = projectileModel;
            lastVisualPosition = model.StartPosition;
            var distance = Vector3.Distance(model.StartPosition, model.TargetPoint);
            var distanceFactor = Mathf.Clamp01(distance / referenceDistance);
            currentDynamicHeight = maxArcHeight * Mathf.Max(distanceFactor, minArcHeightFactor);

            if (!spriteRenderer)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            currentFrameIndex = 0;
            animationTimer = 0f;
        }

        private void Update()
        {
            if (model == null)
                return;

            UpdateAnimation();

            var t = model.TravelProgress;
            var groundPosition = Vector3.Lerp(model.StartPosition, model.TargetPoint, t);
            float yForSorting;

            if (model.Data.isHoming)
            {
                transform.position = model.Position;

                var lookTarget = model.Target?.HitPosition ?? model.Position;
                RotateTowards(lookTarget);

                yForSorting = model.Position.y;
            }
            else
            {
                var height = currentDynamicHeight * 4f * t * (1f - t);

                var currentVisualPosition = groundPosition + Vector3.up * height;
                transform.position = currentVisualPosition;

                ApplyRotation(currentVisualPosition);
                lastVisualPosition = currentVisualPosition;

                yForSorting = groundPosition.y - (model.Target?.Data.hitOffsetY ?? 0) * t;
            }

            spriteRenderer.sortingOrder = yForSorting > model.TowerBaseY ? 0 : 2;
        }

        private void UpdateAnimation()
        {
            if (animationFrames == null || animationFrames.Length == 0)
                return;

            animationTimer += Time.deltaTime;

            if (animationTimer >= 1f / framesPerSecond)
            {
                animationTimer = 0f;
                currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;
                spriteRenderer.sprite = animationFrames[currentFrameIndex];
            }
        }

        private void ApplyRotation(Vector3 currentPos)
        {
            var diff = currentPos - lastVisualPosition;
            if (diff.magnitude < 0.001f)
                return;

            var angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void RotateTowards(Vector3 targetPosition)
        {
            var dir = targetPosition - transform.position;

            if (dir.sqrMagnitude < 0.01f)
                return;

            var direction = dir.normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}