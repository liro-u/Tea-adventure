using UnityEngine;

public class HardStoppingState : StoppingState
{
    public HardStoppingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.movementBrain.movementBrainStatePayload.MovementDecelerationForce = stateMachine.movementBrain.movementData.GroundedData.StopData.HardDecelerationForce;

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce = stateMachine.movementBrain.movementData.AirborneData.JumpData.StrongForce;
    }

    public override void OnAnimationTransitionEvent()
    {
        base.OnAnimationTransitionEvent();
    }

    protected override void OnMove()
    {
    }
}
