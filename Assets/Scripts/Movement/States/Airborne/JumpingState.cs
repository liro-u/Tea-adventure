using System;
using UnityEngine;

public class JumpingState : AirborneState
{
    public override MovementStateId StateId => MovementStateId.Jumping;

    private bool canStartFalling = false;

    public JumpingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.RawStatePayload.MovementSpeedModifier = 0;

        stateMachine.RawStatePayload.MovementDecelerationForce = airborneData.JumpData.DecelerationForce;

        stateMachine.RawStatePayload.RemainingJump -= 1;

        Jump();
    }

    public override void Exit()
    {
        base.Exit();

        canStartFalling = false;
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();

        if (!canStartFalling && IsMovingDown(0f))
        {
            canStartFalling = true;
        }

        if (!canStartFalling || IsMovingUp(0f))
        {
            return;
        }

        stateMachine.ChangeState(stateMachine.FallingState);
    }

    protected override void SimulatePhysicsTick()
    {
        base.SimulatePhysicsTick();

        if (IsMovingUp(0f))
        {
            DecelerateVertically();
        }
    }

    protected override void ResetSprintState()
    {
    }

    private void Jump()
    {
        Vector3 jumpForce = stateMachine.RawStatePayload.CurrentJumpForce;

        Vector3 jumpDirection = stateMachine.RawStatePayload.TargetDirection;

        jumpForce.x *= jumpDirection.x;
        jumpForce.z *= jumpDirection.z;

        ResetVelocity();

        AddForce(jumpForce, ForceMode.Acceleration);
    }
}
