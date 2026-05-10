using UnityEngine;
using Logic.Monster;

namespace View
{
    public class MonsterView : MonoBehaviour
    {
        private MonsterModel model;
        private Vector3 previousPosition;
        
        // [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private Animator animator;
        private Vector2 targetDirection;
        private Vector2 currentSmoothDirection;
        [SerializeField] private float smoothingSpeed = 10f;


        public void Initialize(MonsterModel model)
        {
            this.model = model;
            transform.position = model.WorldPosition;
            previousPosition = model.WorldPosition;
        }
        
        public void UpdateView()
        {
            var currentPosition = model.WorldPosition;
            var direction = currentPosition - previousPosition;

            transform.position = currentPosition;

            if (direction.sqrMagnitude > 0.0001f)
            {
                // UpdateDirection(direction);
                targetDirection = new Vector2(direction.x, direction.y).normalized;
                previousPosition = currentPosition;
            }
            // else 
            // {
            //     // Если монстр стоит, можно занулить параметры, 
            //     // чтобы он перешел в Idle (если ты его добавишь)
            //     animator.SetFloat("MoveX", 0);
            //     animator.SetFloat("MoveY", 0);
            // }
        }
        
        private void Update()
        {
            if (animator == null) return;

            // Плавно перетекаем из текущего направления в целевое
            currentSmoothDirection = Vector2.Lerp(
                currentSmoothDirection, 
                targetDirection, 
                Time.deltaTime * smoothingSpeed
            );

            // Отправляем в аниматор уже сглаженные значения
            animator.SetFloat("MoveX", currentSmoothDirection.x);
            animator.SetFloat("MoveY", currentSmoothDirection.y);
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