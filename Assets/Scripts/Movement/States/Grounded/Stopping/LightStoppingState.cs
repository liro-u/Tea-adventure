using UnityEngine;

public class LightStoppingState : StoppingState
{
    public override MovementStateId StateId => MovementStateId.LightStopping;
    public LightStoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.RawStatePayload.MovementDecelerationForce = groundedData.StopData.LightDecelerationForce;

        stateMachine.RawStatePayload.CurrentJumpForce = airborneData.JumpData.WeakForce;
    }
}
