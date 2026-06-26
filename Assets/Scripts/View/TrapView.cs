using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace View
{
    public class TrapView : MonoBehaviour
    {
        private static readonly int close = Animator.StringToHash("Close");
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        [Header("X-Ray Settings")]
        [SerializeField]
        private GameObject outline;
        [SerializeField]
        private Animator animator;

        private readonly HashSet<Collider2D> overlappingTowers = new();

        private void Update()
        {
            if (overlappingTowers.Count == 0)
            {
                SetOutlineVisible(false);
                return;
            }

            var centerPoint = spriteRenderer.bounds.center;

            var isOccluded = overlappingTowers.Where(tower => transform.position.y > tower.transform.position.y)
                .Any(tower => tower.OverlapPoint(centerPoint));
            SetOutlineVisible(isOccluded);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Tower"))
                overlappingTowers.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Tower"))
                overlappingTowers.Remove(other);
        }

        public void Initialize(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
            SetOutlineVisible(false);
        }

        public void SetOutlineVisible(bool value)
        {
            if (outline != null)
                outline.SetActive(value);
        }

        public void AnimateAndDestroy()
        {
            if (animator != null)
            {
                animator.SetTrigger(close);
                StartCoroutine(WaitAnimationAndDestroyRoutine());
            }
            else
                Destroy(gameObject);
        }

        private IEnumerator WaitAnimationAndDestroyRoutine()
        {
            yield return null;
            var animLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength);
            Destroy(gameObject);
        }
    }
}