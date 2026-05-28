using System.Collections;
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

        private int occludersCount;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Tower"))
            {
                if (transform.position.y > other.transform.position.y)
                {
                    occludersCount++;
                    SetOutlineVisible(occludersCount > 0);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Tower"))
            {
                if (transform.position.y > other.transform.position.y)
                {
                    occludersCount--;
                    if (occludersCount < 0)
                        occludersCount = 0;
                    SetOutlineVisible(occludersCount > 0);
                }
            }
        }

        public void Initialize(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
            SetOutlineVisible(false);
            occludersCount = 0;
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