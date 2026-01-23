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

        stateMachine.RawMovementStatePayload.MovementSpeedModifier = 0;

        stateMachine.RawMovementStatePayload.MovementDecelerationForce = airborneData.JumpData.DecelerationForce;

        stateMachine.RawMovementStatePayload.RemainingJump -= 1;

        stateMachine.RawMovementInputPayload.IsJumping = false;

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

    private void Jump()
    {
        Vector3 jumpForce = stateMachine.RawMovementStatePayload.CurrentJumpForce;

        Vector3 jumpDirection = stateMachine.RawMovementStatePayload.TargetDirection;

        jumpForce.x *= jumpDirection.x;
        jumpForce.z *= jumpDirection.z;

        ResetVelocity();

        AddForce(jumpForce, ForceMode.Acceleration);
    }
}
