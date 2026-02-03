using UnityEngine;
using Unity.Netcode;
using UnityEngine.Splines;


public class StateMachine : IStateMachine
{
    public IState CurrentState { get; set; }

    public StateMachine(IState startingState)
    {
        if (startingState != null)
        {
            ChangeState(startingState);
        }
    }

    #region Public API
    public void ChangeState(IState newState)
    {
        CurrentState?.Exit();

        CurrentState = newState;

        CurrentState.Enter();
    }

    public void Tick(float tickDelta)
    {
        CurrentState.Tick(tickDelta);
    }
    #endregion
}
