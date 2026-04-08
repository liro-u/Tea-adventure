public class LandingState : GroundedState
{
    public LandingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.movementBrain.movementBrainStatePayload.IsLanding = true;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.movementBrain.movementBrainStatePayload.IsLanding = false;
    }
}