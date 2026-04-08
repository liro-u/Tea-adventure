using UnityEngine;

public abstract class State<TStateMachine> : IState
    where TStateMachine : IStateMachine
{
    protected TStateMachine stateMachine;

    public State(TStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Tick(float tickDelta);
}
