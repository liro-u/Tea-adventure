using UnityEngine;

public interface IStateMachine
{
    public IState CurrentState { get; set; }


    public void Tick(float tickDelta);

    public void ChangeState(IState newState);
}
