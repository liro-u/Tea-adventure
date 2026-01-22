public class MovingState : GroundedState
{
    public MovingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawMovementStatePayload.IsMoving = true;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.RawMovementStatePayload.IsMoving = false;
    }
}