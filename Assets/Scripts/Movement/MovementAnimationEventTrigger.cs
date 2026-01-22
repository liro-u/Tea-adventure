using UnityEngine;

public class MovementAnimationEventTrigger : MonoBehaviour
{
    [SerializeField] private MovementStateMachine movementStateMachine;
    [SerializeField] private Animator animator;

    public void TriggerOnAnimationEnterEvent()
    {
        if (IsInAnimationTransition())
        {
            return;
        }

        movementStateMachine.OnAnimationEnterEvent();
    }

    public void TriggerOnAnimationExitEvent()
    {
        if (IsInAnimationTransition())
        {
            return;
        }

        movementStateMachine.OnAnimationExitEvent();
    }

    public void TriggerOnAnimationTransitionEvent()
    {
        if (IsInAnimationTransition())
        {
            return;
        }

        movementStateMachine.OnAnimationTransitionEvent();
    }

    private bool IsInAnimationTransition(int layerIndex = 0)
    {
        return animator.IsInTransition(layerIndex);
    }
}
