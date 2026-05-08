using Logic.Unit;
using UnityEngine;

namespace View
{
    public class UnitView : MonoBehaviour
    {
        private UnitModel model;
        
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private UnitBuffsViewManager buffsView;

        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int AttackType = Animator.StringToHash("attackType");

        public void Initialize(UnitModel modelToInitialize, UnitBuffsViewManager buffsViewManager)
        {
            model = modelToInitialize;
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            transform.position = modelToInitialize.WorldPosition;
            buffsView = buffsViewManager;

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

        public void SetPosition(Vector3 worldPos)
        {
            transform.position = worldPos;
        }

        private void Update()
        {
            if (model == null)
                return;
            transform.position = model.WorldPosition;
            var isMoving = model.CurrentDirection.sqrMagnitude > 0.01f;

            if (animator != null)
            {
                animator.SetBool(IsMoving, isMoving);
                animator.SetInteger(AttackType, model.AttackType);
            }

            // Разворот спрайта по направлению движения
            if (spriteRenderer != null && isMoving)
            {
                spriteRenderer.flipX = model.CurrentDirection.x < 0;
            }
            
        }
    }

}