
using UnityEngine;

public class GroundedState : MovementState
{
    public GroundedState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawMovementStatePayload.IsGrounded = true;

        stateMachine.RawMovementStatePayload.RemainingJump = airborneData.JumpData.MaxConsecutiveJump;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.RawMovementStatePayload.IsGrounded = false;
    }

    protected virtual void OnMove()
    {
        if (stateMachine.currentInput.IsRunning)
        {
            stateMachine.ChangeState(stateMachine.RunningState);

            return;
        }

        stateMachine.ChangeState(stateMachine.WalkingState);
    }

    protected override void SimulateTick()
    {
        base.SimulateTick();
    }

    protected override void OnContactWithGroundExited()
    {
        OnFall();
    }
    protected virtual void OnFall()
    {
        //stateMachine.ChangeState(stateMachine.FallingState);
    }

}