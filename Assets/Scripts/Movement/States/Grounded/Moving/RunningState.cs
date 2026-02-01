using UnityEngine;
public class RunningState : MovingState
{
    public override MovementStateId StateId => MovementStateId.Running;

    private float startTime;

    public RunningState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawStatePayload.MovementSpeedModifier = groundedData.RunData.SpeedModifier;

        stateMachine.RawStatePayload.CurrentJumpForce = airborneData.JumpData.MediumForce;

        base.Enter();

        startTime = Time.time;
    }

    public override void Exit()
    {
        base.Exit();
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();

        if (!stateMachine.RawStatePayload.ShouldWalk)
        {
            return;
        }

        if (Time.time < startTime + groundedData.SprintData.RunToWalkTime)
        {
            return;
        }

        StopRunning();
    }

    private void StopRunning()
    {
        if (stateMachine.currentInputPayload.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.IdlingState);

            return;
        }

        stateMachine.ChangeState(stateMachine.WalkingState);
    }

    protected override void OnMoveCanceled()
    {
        stateMachine.ChangeState(stateMachine.MediumStoppingState);

        base.OnMoveCanceled();
    }

    protected override void OnWalkToggleStarted()
    {
        base.OnWalkToggleStarted();

        stateMachine.ChangeState(stateMachine.WalkingState);
    }
}