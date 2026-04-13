/// <summary>
/// Base class for all moving platform states.
/// Provides typed access to the shared state machine via the generic State base.
/// </summary>
public abstract class MovingPlatformState : State<MovingPlatformStateMachine>
{
    protected MovingPlatformState(MovingPlatformStateMachine stateMachine) : base(stateMachine) { }
}
