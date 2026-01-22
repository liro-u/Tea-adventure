
public class GroundedState : MovementState
{
    public GroundedState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawMovementStatePayload.IsGrounded = true;

        stateMachine.RawMovementStatePayload.RemainingJump = 2;

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
}