/// <summary>
/// HFSM for the moving platform. Owns all state instances and injects the
/// current input payload each tick so states never reach outside the state machine.
/// </summary>
public class MovingPlatformStateMachine : StateMachine
{
    public readonly IMovingPlatformBrain Brain;

    /// <summary>Input for the current tick, injected by BrainCore before Tick().</summary>
    public MovingPlatformInputPayload CurrentInput { get; private set; }

    public readonly IdlePlatformState          IdleState;
    public readonly MovingPlatformMovingState  MovingState;
    public readonly WaitingPlatformState       WaitingState;

    public MovingPlatformStateMachine(IMovingPlatformBrain brain) : base(null)
    {
        Brain        = brain;
        IdleState    = new IdlePlatformState(this);
        MovingState  = new MovingPlatformMovingState(this);
        WaitingState = new WaitingPlatformState(this);
        ChangeState(IdleState);
    }

    public void Tick(float dt, MovingPlatformInputPayload input)
    {
        CurrentInput = input;
        Tick(dt);
    }
}
