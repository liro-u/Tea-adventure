using System;
using UnityEngine;

public class JumpingState : AirborneState
{
    private bool canStartFalling = false;

    public JumpingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.movementBrain.movementBrainStatePayload.MovementSpeedModifier = 0;

        stateMachine.movementBrain.movementBrainStatePayload.MovementDecelerationForce = stateMachine.movementBrain.movementData.AirborneData.JumpData.DecelerationForce;

        stateMachine.movementBrain.movementBrainStatePayload.RemainingJump -= 1;

        Jump();
    }

    public override void Exit()
    {
        base.Exit();

        canStartFalling = false;
    }

    public override void Tick(float tickDelta)
    {
        base.Tick(tickDelta);

        if (!canStartFalling && stateMachine.movementBrain.movementMotor.IsMovingDown(0f))
        {
            canStartFalling = true;
        }

        if (canStartFalling && !stateMachine.movementBrain.movementMotor.IsMovingUp(0f))
        {
            stateMachine.ChangeState(stateMachine.FallingState);
        }

        if (stateMachine.movementBrain.movementMotor.IsMovingUp(0f))
        {
            DecelerateVertically();
        }
    }

    protected override void ResetSprintState()
    {
    }

    private void Jump()
    {
        Vector3 jumpForce = stateMachine.movementBrain.movementBrainStatePayload.CurrentJumpForce;

        Vector3 jumpDirection = stateMachine.movementBrain.movementMotor.TargetDirection;

        jumpForce.x *= jumpDirection.x;
        jumpForce.z *= jumpDirection.z;

        stateMachine.movementBrain.movementMotor.ResetVelocity();

        stateMachine.movementBrain.movementMotor.AddForce(jumpForce, ForceMode.VelocityChange);
    }
}
