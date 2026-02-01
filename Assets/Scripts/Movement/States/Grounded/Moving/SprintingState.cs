using UnityEngine;
using UnityEngine.InputSystem;

public class SprintingState : MovingState
{
    public override MovementStateId StateId => MovementStateId.Sprinting;

    private float startTime;

    private bool keepSprinting;
    private bool shouldResetSprintState;

    public SprintingState(MovementStateMachine movementStateMachine) : base(movementStateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawStatePayload.ShouldSprint = true;

        stateMachine.RawStatePayload.MovementSpeedModifier = groundedData.SprintData.SpeedModifier;

        stateMachine.RawStatePayload.CurrentJumpForce = airborneData.JumpData.StrongForce;

        base.Enter();

        startTime = Time.time;

        shouldResetSprintState = true;

        if (!stateMachine.RawStatePayload.ShouldSprint)
        {
            keepSprinting = false;
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (shouldResetSprintState)
        {
            keepSprinting = false;

            stateMachine.RawStatePayload.ShouldSprint = false;
        }
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();

        if (stateMachine.currentInputPayload.IsSprinting)
        {
            OnSprintPerformed();
        }

        if (keepSprinting)
        {
            return;
        }

        if (Time.time < startTime + groundedData.SprintData.SprintToRunTime)
        {
            return;
        }

        StopSprinting();
    }

    private void OnSprintPerformed()
    {
        keepSprinting = true;

        stateMachine.RawStatePayload.ShouldSprint = true;
    }

    private void StopSprinting()
    {
        if (stateMachine.currentInputPayload.MoveInput == Vector2.zero)
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
