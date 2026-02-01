public class LandingState : GroundedState
{
    public LandingState(MovementStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.RawStatePayload.IsLanding = true;

        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        stateMachine.RawStatePayload.IsLanding = false;
    }
}