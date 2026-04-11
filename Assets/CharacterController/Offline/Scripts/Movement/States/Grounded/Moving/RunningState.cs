using UnityEngine;

public class RunningState : MovingState
{
    public RunningState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier =
            stateMachine.movementBrain.movementData.GroundedData.RunData.SpeedModifier;

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce =
            stateMachine.movementBrain.movementData.AirborneData.JumpData.MediumForce;

        stateMachine.movementBrain.movementBrainStatePayload.StateTimer = 0f;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (!stateMachine.movementBrain.movementBrainStatePayload.ShouldWalk)
            return;

        stateMachine.movementBrain.movementBrainStatePayload.StateTimer += tickDelta;

        if (stateMachine.movementBrain.movementBrainStatePayload.StateTimer <
            stateMachine.movementBrain.movementData.GroundedData.SprintData.RunToWalkTime)
            return;

        StopRunning();
    }

    private void StopRunning()
    {
        if (stateMachine.CurrentInput.MoveInput == Vector2.zero)
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
