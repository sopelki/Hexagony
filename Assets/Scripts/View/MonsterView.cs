using UnityEngine;
using Logic.Monster;

namespace View
{
    public class MonsterView : MonoBehaviour
    {
        private static readonly int moveX = Animator.StringToHash("MoveX");
        private static readonly int moveY = Animator.StringToHash("MoveY");
        private MonsterModel model;
        private Vector3 previousPosition;

        [SerializeField]
        private Animator animator;
        private Vector2 targetDirection;
        private Vector2 currentSmoothDirection;
        [SerializeField]
        private float smoothingSpeed = 10f;
        private Vector3 visualOffset;


        public void Initialize(MonsterModel model, float visualOffset)
        {
            this.model = model;
            transform.position = model.WorldPosition;
            previousPosition = model.WorldPosition;
            this.visualOffset = new Vector3(0, model.Data.visualOffsetY, 0);
        }

        private static readonly int isMovingHash = Animator.StringToHash("IsMoving");

        public void UpdateView()
        {
            var logicalPosition = model.WorldPosition;
            var direction = logicalPosition - previousPosition;

            transform.position = logicalPosition + visualOffset;

            // IsMoving для Юли Курановой
            // "Добавьте параметр типа Bool с именем IsMoving.
            // В переходах (Transitions) между Idle и Walk используйте этот параметр."
            var isMoving = direction.sqrMagnitude > 0.0001f;
            animator.SetBool(isMovingHash, isMoving);

            if (isMoving)
            {
                targetDirection = new Vector2(direction.x, direction.y).normalized;
                previousPosition = logicalPosition;
            }
        }

        private void Update()
        {
            if (!animator) return;

            currentSmoothDirection = Vector2.Lerp(
                currentSmoothDirection,
                targetDirection,
                Time.deltaTime * smoothingSpeed
            );

            animator.SetFloat(moveX, currentSmoothDirection.x);
            animator.SetFloat(moveY, currentSmoothDirection.y);
        }

        // private void UpdateDirection(Vector3 direction)
        // {
        //     direction.Normalize();
        //     animator.SetFloat("MoveX", direction.x, 0.1f, Time.deltaTime);
        //     animator.SetFloat("MoveY", direction.y, 0.1f, Time.deltaTime);
        // }

        // private void Update()
        // {
        //     if (currentFrames == null || currentFrames.Length == 0) return;
        //     
        //     timer += Time.deltaTime;
        //
        //     if (timer >= animationData.ticksPerFrame)
        //     {
        //         timer = 0;
        //         currentFrameIndex = (currentFrameIndex + 1) % currentFrames.Length;
        //         spriteRenderer.sprite = currentFrames[currentFrameIndex];
        //         Debug.Log($"Смена кадра: {currentFrameIndex} у объекта {gameObject.name}");
        //     }
        // }
        //
        // private void UpdateDirection(Vector3 direction)
        // {
        //     direction.Normalize();
        //     var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //
        //     Sprite[] nextFrames;
        //     
        //     if (angle >= -22.5f && angle < 22.5f)
        //         nextFrames = animationData.right;
        //     else if (angle >= 22.5f && angle < 67.5f)
        //         nextFrames = animationData.upRight;
        //     else if (angle >= 67.5f && angle < 112.5f)
        //         nextFrames = animationData.up;
        //     else if (angle >= 112.5f && angle < 157.5f)
        //         nextFrames = animationData.upLeft;
        //     else if (angle >= 157.5f || angle < -157.5f)
        //         nextFrames = animationData.left;
        //     else if (angle >= -157.5f && angle < -112.5f)
        //         nextFrames = animationData.downLeft;
        //     else if (angle >= -112.5f && angle < -67.5f)
        //         nextFrames = animationData.down;
        //     else
        //         nextFrames = animationData.downRight;
        //     
        //     if (currentFrames != nextFrames)
        //     {
        //         currentFrames = nextFrames;
        //         currentFrameIndex = 0;
        //         timer = 0; 
        //         
        //         if (currentFrames != null && currentFrames.Length > 0)
        //             spriteRenderer.sprite = currentFrames[0];
        //     }
        // }
    }
}