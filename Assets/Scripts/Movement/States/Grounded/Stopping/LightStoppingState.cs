using UnityEngine;

public class LightStoppingState : StoppingState
{
    public override MovementStateId StateId => MovementStateId.LightStopping;
    public LightStoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }
}
