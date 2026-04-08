using UnityEngine;

public class WalkingState : MovingState
{
    public WalkingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = stateMachine.movementBrain.movementData.GroundedData.WalkData.SpeedModifier;

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce = stateMachine.movementBrain.movementData.AirborneData.JumpData.WeakForce;

        base.Enter();
    }

    protected override void OnMoveCanceled()
    {
        stateMachine.ChangeState(stateMachine.LightStoppingState);

        base.OnMoveCanceled();
    }

    protected override void OnWalkToggleStarted()
    {
        base.OnWalkToggleStarted();

        stateMachine.ChangeState(stateMachine.RunningState);
    }
}