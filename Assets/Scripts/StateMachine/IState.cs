using UnityEngine;

public interface IState<TInput, TState>
{
    public void Enter();
    public void Exit();
    //public void HandleInput();
    public TState Simulate(TInput input);
    //public void OnTriggerEnter(Collider collider);
    //public void OnTriggerExit(Collider collider);
}
