using System;
using Logic.Unit;
using UnityEngine;

namespace View
{
    public class UnitView : MonoBehaviour
    {
        private UnitModel model;
        private Animator animator;
        private UnitBuffsViewManager buffsView;

        private static readonly int moveX = Animator.StringToHash("MoveX");
        private static readonly int moveY = Animator.StringToHash("MoveY");
        private static readonly int lastMoveX = Animator.StringToHash("LastMoveX");
        private static readonly int lastMoveY = Animator.StringToHash("LastMoveY");
        private static readonly int isMoving = Animator.StringToHash("IsMoving");
        private static readonly int isDamaged = Animator.StringToHash("IsDamaged");
        private static readonly int isDead = Animator.StringToHash("IsDead");
        private static readonly int isAttacking = Animator.StringToHash("IsAttacking");

        private Vector3 previousPosition;
        private Vector2 targetDirection;
        private Vector2 currentSmoothDirection;
        private Vector3 visualOffset;

        [SerializeField]
        private float smoothingSpeed = 10f;
        private bool isDeadAnimationPlaying;

        public Action<UnitModel> OnDeathAnimationFinished;

        public void Initialize(UnitModel modelToInitialize, UnitBuffsViewManager buffsViewManager, float visualOffset)
        {
            model = modelToInitialize;
            animator = GetComponent<Animator>();
            GetComponent<SpriteRenderer>();
            transform.position = modelToInitialize.WorldPosition;
            previousPosition = modelToInitialize.WorldPosition;
            buffsView = buffsViewManager;
            this.visualOffset = new Vector3(0, model.UnitData.visualOffsetY, 0);

            model.OnAttack += HandleAttack;
            model.OnDamaged += HandleDamaged;
            model.OnDied += HandleDeath;

            ApplyBuffGlows();
        }

        private void ApplyBuffGlows()
        {
            if (model.ActiveBuffs == null || buffsView == null) return;

            foreach (var buff in model.ActiveBuffs)
            {
                var prefab = buffsView.GetPrefabForBuff(buff);
                if (prefab != null)
                {
                    var glow = Instantiate(prefab, transform);
                    glow.transform.localPosition = prefab.transform.localPosition;
                }
            }
        }

        public void SetPosition(Vector3 worldPos) => transform.position = worldPos;

        private void Update()
        {
            if (model == null || !animator || isDeadAnimationPlaying)
                return;

            currentSmoothDirection = Vector2.Lerp(
                currentSmoothDirection,
                targetDirection,
                Time.deltaTime * smoothingSpeed
            );

            animator.SetFloat(moveX, currentSmoothDirection.x);
            animator.SetFloat(moveY, currentSmoothDirection.y);
        }

        private void HandleAttack() => animator?.SetTrigger(isAttacking);
        private void HandleDamaged() => animator?.SetBool(isDamaged, true);
        private void HandleDeath() => UpdateView();

        public void UpdateView()
        {
            if (model.IsDead)
            {
                if (!isDeadAnimationPlaying)
                {
                    isDeadAnimationPlaying = true;
                    if (animator)
                    {
                        animator.SetBool(isMoving, false);
                        animator.SetTrigger(isDead);
                    }
                }
                return;
            }

            var logicalPosition = model.WorldPosition;
            var direction = logicalPosition - previousPosition;
            var moving = direction.sqrMagnitude > 0.0001f;

            transform.position = logicalPosition + visualOffset;
            
            if (animator)
                animator.SetBool(isMoving, moving);

            if (moving)
            {
                targetDirection = SnapTo4Directions(direction);
                if (animator)
                {
                    animator.SetFloat(lastMoveX, targetDirection.x);
                    animator.SetFloat(lastMoveY, targetDirection.y);
                }
                previousPosition = logicalPosition;
            }
        }

        private static Vector2 SnapTo4Directions(Vector3 direction)
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

        private void OnDestroy()
        {
            if (model != null)
            {
                model.OnAttack -= HandleAttack;
                model.OnDamaged -= HandleDamaged;
                model.OnDied -= HandleDeath;
            }
        }

        public void OnDeathAnimationEnd()
        {
            OnDeathAnimationFinished?.Invoke(model);
            Destroy(gameObject);
        }
    }
}