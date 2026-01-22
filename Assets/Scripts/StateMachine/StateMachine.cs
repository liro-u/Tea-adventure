using UnityEngine;
using Unity.Netcode;
using UnityEngine.Splines;


public abstract class StateMachine<TInput, TState> : ClientPredictionNetworkBehaviour<TInput, TState>
    where TInput : struct, ITickPayload
    where TState : struct, ITickPayload
{
    public IState<TInput, TState> currentState { get; protected set; }
    public TInput currentInput { get; private set; }

    public void ChangeState(IState<TInput, TState> newState)
    {
        currentState?.Exit();

        currentState = newState;

        currentState.Enter();
    }


    protected override TState Simulate(TInput input)
    {
        currentInput = input;

        return currentState.Simulate();
    }

    public void OnAnimationEnterEvent()
    {
        currentState?.OnAnimationEnterEvent();
    }

    public void OnAnimationExitEvent()
    {
        currentState?.OnAnimationExitEvent();
    }

    public void OnAnimationTransitionEvent()
    {
        currentState?.OnAnimationTransitionEvent();
    }

    //public void OnTriggerEnter(Collider collider)
    //{
    //    currentState.OnTriggerEnter(collider);
    //}

    //public void OnTriggerExit(Collider collider)
    //{
    //    currentState.OnTriggerExit(collider);
    //}
}
