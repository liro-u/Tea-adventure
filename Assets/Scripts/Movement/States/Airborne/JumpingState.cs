using System;
using UnityEngine;

public class JumpingState : AirborneState
{
    public override MovementStateId StateId => MovementStateId.Jumping;
    public JumpingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.RawMovementInputPayload.IsJumping = false;

        stateMachine.RawMovementStatePayload.MovementSpeedModifier = 0;

        stateMachine.RawMovementStatePayload.RemainingJump -= 1;

        Jump();
    }

    private void Jump()
    {
        // add jump
    }
}
