using UnityEngine;

public interface IState<TInput, TState>
{
    public void Enter();
    public void Exit();
    public TState Simulate();

    //public void OnTriggerEnter(Collider collider);
    //public void OnTriggerExit(Collider collider);

    public void OnAnimationEnterEvent();
    public void OnAnimationExitEvent();
    public void OnAnimationTransitionEvent();
}
