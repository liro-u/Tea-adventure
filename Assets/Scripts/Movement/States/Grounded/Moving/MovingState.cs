public class MovingState : GroundedState
{
    public MovingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawStatePayload.IsMoving = true;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.RawStatePayload.IsMoving = false;
    }
}