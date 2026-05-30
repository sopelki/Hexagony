using System;
using Logic.Monster;
using UnityEngine;

namespace View
{
    public class MonsterView : MonoBehaviour
    {
        private static readonly int moveX = Animator.StringToHash("MoveX");
        private static readonly int moveY = Animator.StringToHash("MoveY");
        private static readonly int lastMoveX = Animator.StringToHash("LastMoveX");
        private static readonly int lastMoveY = Animator.StringToHash("LastMoveY");
        private static readonly int isMoving = Animator.StringToHash("IsMoving");
        private static readonly int isAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int isDamaged = Animator.StringToHash("IsDamaged");
        private static readonly int isDead = Animator.StringToHash("IsDead");

        [SerializeField]
        private Animator animator;
        [SerializeField]
        private float smoothingSpeed = 10f;
        private Vector2 currentSmoothDirection;
        private bool isDeadAnimationPlaying;
        private MonsterModel model;
        public Action<MonsterModel> OnDeathAnimationFinished;
        private Vector3 previousPosition;
        private Vector2 targetDirection;
        private Vector3 visualOffset;


        private void Update()
        {
            if (!animator || isDeadAnimationPlaying) return;

            currentSmoothDirection = Vector2.Lerp(
                currentSmoothDirection,
                targetDirection,
                Time.deltaTime * smoothingSpeed
            );

            animator.SetFloat(moveX, currentSmoothDirection.x);
            animator.SetFloat(moveY, currentSmoothDirection.y);
        }

        private void OnDestroy()
        {
            if (model != null)
            {
                model.OnAttack -= HandleAttack;
                model.OnDamaged -= HandleDamaged;
                model.OnDied -= HandleDeath;
            }
        }


        public void Initialize(MonsterModel monsterModel, float visualOffset)
        {
            model = monsterModel;
            transform.position = model.WorldPosition;
            previousPosition = model.WorldPosition;
            this.visualOffset = new Vector3(0, model.Data.visualOffsetY, 0);

            model.OnAttack += HandleAttack;
            model.OnDied += HandleDeath;
            model.OnDamaged += HandleDamaged;
        }

        public void UpdateView()
        {
            if (model.IsDead)
            {
                if (!isDeadAnimationPlaying)
                {
                    isDeadAnimationPlaying = true;
                    animator.SetBool(isMoving, false);
                    animator.SetBool(isDead, true);
                }
                return;
            }

            var logicalPosition = model.WorldPosition;
            var direction = logicalPosition - previousPosition;
            var moving = direction.sqrMagnitude > 0.0001f;

            transform.position = logicalPosition + visualOffset;
            animator.SetBool(isMoving, moving);

            if (moving)
            {
                targetDirection = SnapTo4Directions(direction);
                animator.SetFloat(lastMoveX, targetDirection.x);
                animator.SetFloat(lastMoveY, targetDirection.y);

                previousPosition = logicalPosition;
            }
        }

        private Vector2 SnapTo4Directions(Vector3 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            return angle switch
            {
                >= -45f and < 45f => new Vector2(1, 0),
                >= 45f and < 135f => new Vector2(0, 1),
                >= -135f and < -45f => new Vector2(0, -1),
                _ => new Vector2(-1, 0)
            };
        }

        private void HandleAttack()
        {
            animator.SetTrigger(isAttacking);
        }

        private void HandleDamaged()
        {
            animator.SetBool(isDamaged, true);
        }

        private void HandleDeath()
        {
            animator.SetBool(isDead, true);
        }


        public void OnDeathAnimationEnd()
        {
            OnDeathAnimationFinished?.Invoke(model);
            Destroy(gameObject);
        }
    }
}