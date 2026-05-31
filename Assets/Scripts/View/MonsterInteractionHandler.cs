using Logic.Monster;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View
{
    public class MonsterInteractionHandler : MonoBehaviour
    {
        [Header("Настройки урона")]
        [SerializeField]
        private int damagePerClick = 5;
        [SerializeField]
        private float clickCooldown = 0.5f;

        private MonsterModel model;
        private float nextAllowedClickTime;

        public void Setup(MonsterModel monsterModel) => model = monsterModel;

        public bool IsModelDead() => model == null || model.IsDead;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (model == null || model.IsDead) return;

            if (Time.unscaledTime >= nextAllowedClickTime)
            {
                model.TakeDamage(damagePerClick);

                nextAllowedClickTime = Time.unscaledTime + clickCooldown;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (model is { IsDead: false } && Misc.GlobalCursorManager.Instance)
                Misc.GlobalCursorManager.Instance.SetAttack();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Misc.GlobalCursorManager.Instance != null)
                Misc.GlobalCursorManager.Instance.SetDefault();
        }
    }
}