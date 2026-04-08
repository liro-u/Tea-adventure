using UnityEngine;
using UnityEngine.InputSystem;

public class SprintingState : MovingState
{
    private float startTime;

    private bool keepSprinting;
    private bool shouldResetSprintState;

    public SprintingState(MovementStateMachine movementStateMachine) : base(movementStateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint = true;

        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = stateMachine.movementBrain.movementData.GroundedData.SprintData.SpeedModifier;

        stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce = stateMachine.movementBrain.movementData.AirborneData.JumpData.StrongForce;

        base.Enter();

        startTime = Time.time;

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

        if (stateMachine.movementBrain.movementInputProvider.InputPayload.IsSprinting)
        {
            OnSprintPerformed();
        }

        if (keepSprinting)
        {
            return;
        }

        if (Time.time < startTime + stateMachine.movementBrain.movementData.GroundedData.SprintData.SprintToRunTime)
        {
            return;
        }

        StopSprinting();
    }

    private void OnSprintPerformed()
    {
        keepSprinting = true;

        stateMachine.movementBrain.movementBrainStatePayload.ShouldSprint = true;
    }

    private void StopSprinting()
    {
        if (stateMachine.movementBrain.movementInputProvider.InputPayload.MoveInput == Vector2.zero)
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
