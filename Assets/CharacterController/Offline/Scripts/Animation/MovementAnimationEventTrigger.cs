using UnityEngine;
using System;

public class MovementAnimationEventTrigger : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public event Action OnAnimationEnterEvent;
    public event Action OnAnimationExitEvent;
    public event Action OnAnimationTransitionEvent;

    public void TriggerOnAnimationEnterEvent()
    {
        if (IsInAnimationTransition())
        {
            return;
        }

        OnAnimationEnterEvent.Invoke();
    }

    public void TriggerOnAnimationExitEvent()
    {
        if (IsInAnimationTransition())
        {
            return;
        }

        OnAnimationExitEvent.Invoke();
    }

    public void TriggerOnAnimationTransitionEvent()
    {
        if (IsInAnimationTransition())
        {
            return;
        }

        OnAnimationTransitionEvent.Invoke();
    }

    private bool IsInAnimationTransition(int layerIndex = 0)
    {
        return animator.IsInTransition(layerIndex);
    }
}
