using System;

public class MovementStateMachine : StateMachine
{
    public IMovementBrain movementBrain;

    // Injected at the start of every Tick call.
    // States read input from here instead of from the brain's input provider,
    // so the same state code runs unchanged during reconciliation replay.
    public PlayerInputPayload CurrentInput { get; private set; }

    public IdlingState IdlingState;

    public WalkingState WalkingState;
    public RunningState RunningState;
    public SprintingState SprintingState;

    public LightStoppingState LightStoppingState;
    public MediumStoppingState MediumStoppingState;
    public HardStoppingState HardStoppingState;

    public LightLandingState LightLandingState;
    public HardLandingState HardLandingState;
    public RollingState RollingState;

    public JumpingState JumpingState;
    public FallingState FallingState;

    public MovementStateMachine(IMovementBrain movementBrain) : base(null)
    {
        this.movementBrain = movementBrain;

        IdlingState = new IdlingState(this);

        WalkingState = new WalkingState(this);
        RunningState = new RunningState(this);
        SprintingState = new SprintingState(this);

        LightStoppingState = new LightStoppingState(this);
        MediumStoppingState = new MediumStoppingState(this);
        HardStoppingState = new HardStoppingState(this);

        LightLandingState = new LightLandingState(this);
        HardLandingState = new HardLandingState(this);
        RollingState = new RollingState(this);

        JumpingState = new JumpingState(this);
        FallingState = new FallingState(this);

        ChangeState(IdlingState);
    }

    public void Tick(float dt, PlayerInputPayload input)
    {
        CurrentInput = input;
        Tick(dt);
    }
}
