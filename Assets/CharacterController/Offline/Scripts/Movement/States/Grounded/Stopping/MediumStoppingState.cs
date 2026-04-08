using UnityEngine;

public class MediumStoppingState : StoppingState
{
    public MediumStoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.movementBrain.movementBrainStatePayload.MovementDecelerationForce = stateMachine.movementBrain.movementData.GroundedData.StopData.MediumDecelerationForce;

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce = stateMachine.movementBrain.movementData.AirborneData.JumpData.MediumForce;
    }
}
