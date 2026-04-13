/// <summary>
/// Platform is paused at a waypoint for PlatformData.WaitDuration seconds.
/// The next target has already been set by MovingPlatformMovingState before this state is entered,
/// so on exit the platform moves directly toward it.
/// </summary>
public class WaitingPlatformState : MovingPlatformState
{
    public WaitingPlatformState(MovingPlatformStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.Brain.StatePayload.WaitTimer = stateMachine.Brain.PlatformData.WaitDuration;
        stateMachine.Brain.Motor.ResetVelocity();
    }

    public override void Tick(float tickDelta)
    {
        stateMachine.Brain.StatePayload.WaitTimer -= tickDelta;
        if (stateMachine.Brain.StatePayload.WaitTimer <= 0f)
            stateMachine.ChangeState(stateMachine.MovingState);
    }

    public override void Exit() { }
}
