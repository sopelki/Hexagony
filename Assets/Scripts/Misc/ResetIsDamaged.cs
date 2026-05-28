using UnityEngine;

namespace Misc
{
    public class ResetIsDamaged : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("IsDamaged", false);
        }
    }
}