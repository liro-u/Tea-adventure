public class MovingState : GroundedState
{
    public MovingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.IsMoving = true;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.movementBrain.movementBrainStatePayload.IsMoving = false;
    }
}