using System.Collections;
using System.Collections.Generic;
using Logic.Trap;
using UnityEngine;

namespace View
{
    public class TrapView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Header("X-Ray Settings")]
        [SerializeField] private GameObject outline;
        [SerializeField] private Animator animator;
        
        private int occludersCount = 0;
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
                animator.SetTrigger("Close");
                StartCoroutine(WaitAnimationAndDestroyRoutine());
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private IEnumerator WaitAnimationAndDestroyRoutine()
        {
            yield return null; 
            float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength);
            Destroy(gameObject);
        }
        
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

    }
}