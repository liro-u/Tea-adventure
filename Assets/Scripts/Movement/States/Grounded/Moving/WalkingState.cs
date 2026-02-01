using UnityEngine;

public class WalkingState : MovingState
{
    public override MovementStateId StateId => MovementStateId.Walking;

    public WalkingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawStatePayload.MovementSpeedModifier = groundedData.WalkData.SpeedModifier;

        stateMachine.RawStatePayload.CurrentJumpForce = airborneData.JumpData.WeakForce;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

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