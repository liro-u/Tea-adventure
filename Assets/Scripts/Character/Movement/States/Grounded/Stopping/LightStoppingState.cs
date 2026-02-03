using UnityEngine;

public class LightStoppingState : StoppingState
{
    public LightStoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.movementBrain.movementBrainStatePayload.MovementDecelerationForce = stateMachine.movementBrain.movementData.GroundedData.StopData.LightDecelerationForce;

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce = stateMachine.movementBrain.movementData.AirborneData.JumpData.WeakForce;
    }
}
