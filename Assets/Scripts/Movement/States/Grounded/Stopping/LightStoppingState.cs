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

        stateMachine.RawMovementStatePayload.MovementDecelerationForce = groundedData.StopData.LightDecelerationForce;

        stateMachine.RawMovementStatePayload.CurrentJumpForce = airborneData.JumpData.WeakForce;
    }
}
