using UnityEngine;

public class SprintingState : MovingState
{
    private bool keepSprinting;
    private bool shouldResetSprintState;

    public SprintingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint = true;
        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier =
            stateMachine.movementBrain.movementData.GroundedData.SprintData.SpeedModifier;
        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce =
            stateMachine.movementBrain.movementData.AirborneData.JumpData.StrongForce;
        stateMachine.movementBrain.movementBrainStatePayload.StateTimer = 0f;

        base.Enter();

        keepSprinting = false;
        shouldResetSprintState = true;
    }

    public override void Exit()
    {
        base.Exit();

        if (shouldResetSprintState)
        {
            keepSprinting = false;
            stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint = false;
        }
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (stateMachine.CurrentInput.IsSprinting)
            OnSprintPerformed();

        if (keepSprinting)
            return;

        stateMachine.movementBrain.movementBrainStatePayload.StateTimer += tickDelta;

        if (stateMachine.movementBrain.movementBrainStatePayload.StateTimer <
            stateMachine.movementBrain.movementData.GroundedData.SprintData.SprintToRunTime)
            return;

        StopSprinting();
    }

    private void OnSprintPerformed()
    {
        keepSprinting = true;
        stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint = true;
    }

    private void StopSprinting()
    {
        if (stateMachine.CurrentInput.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.IdlingState);
            return;
        }

        stateMachine.ChangeState(stateMachine.RunningState);
    }

    protected override void OnMoveCanceled()
    {
        stateMachine.ChangeState(stateMachine.HardStoppingState);
        base.OnMoveCanceled();
    }

    protected override void OnJumpStarted()
    {
        shouldResetSprintState = false;
        base.OnJumpStarted();
    }

    protected override void OnFall()
    {
        shouldResetSprintState = false;
        base.OnFall();
    }
}
