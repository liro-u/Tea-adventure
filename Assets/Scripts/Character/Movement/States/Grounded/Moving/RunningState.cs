using UnityEngine;
public class RunningState : MovingState
{
    private float startTime;

    public RunningState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = stateMachine.movementBrain.movementData.GroundedData.RunData.SpeedModifier;

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce = stateMachine.movementBrain.movementData.AirborneData.JumpData.MediumForce;

        base.Enter();

        startTime = Time.time;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (!stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk)
        {
            return;
        }

        if (Time.time < startTime + stateMachine.movementBrain.movementData.GroundedData.SprintData.RunToWalkTime)
        {
            return;
        }

        StopRunning();
    }

    private void StopRunning()
    {
        if (stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput == Vector2.zero)
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