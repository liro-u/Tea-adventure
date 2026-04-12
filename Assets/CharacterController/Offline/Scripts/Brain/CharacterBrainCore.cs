using UnityEngine;

/// <summary>
/// Pure C# brain for the player character — no MonoBehaviour, no NetworkBehaviour.
///
/// Implements ISimulatableEntity directly so it can be registered with WorldSimulation
/// without the MonoBehaviour/NetworkBehaviour wrapper acting as a pass-through.
/// Wrappers (CharacterBrain, NetworkCharacterBrain) create an instance in Awake,
/// call Register/Unregister at the appropriate lifecycle points, and drive
/// OnUpdate each frame.
///
/// Fixed tick flow:
///   WorldSimulation.Tick → ISimulatableEntity.SimulateTick(dt) → OnSimulateTick(dt)
///   OnSimulateTick pulls live input and calls SimulateTick(dt, input).
///   NetworkCharacterBrainCore overrides OnSimulateTick to buffer, replay, and dispatch RPC callbacks.
///
/// Per-frame flow:
///   Wrapper.Update → OnUpdate(dt)  (input poll, animation, camera)
/// </summary>
public class CharacterBrainCore : ISimulatableEntity, IMovementBrain,
    ISimulatable<PlayerInputPayload, PlayerStateSnapshot>
{
    // ── IMovementBrain ────────────────────────────────────────────────────────

    public MovementSO                        movementData                  { get; }
    public IMovementBrainStatePayload        movementBrainStatePayload     { get; set; }
    public IAdvancedMotor                    movementMotor                 { get; }
    public MovementStateMachine              movementStateMachine          { get; }
    public CharacterAnimatorController       characterAnimatorController   { get; }
    public MovementAnimationEventTrigger     MovementAnimationEventTrigger { get; }

    private readonly IInputProvider<PlayerInputPayload> inputProvider;
    private readonly CharacterCamera                    characterCamera;

    // ── Construction ──────────────────────────────────────────────────────────

    public CharacterBrainCore(
        CharacterController           characterController,
        MovementSO                    movementSO,
        Animator                      animator,
        Transform                     meshTransform,
        MovementAnimationEventTrigger movementAnimationEventTrigger,
        Transform                     cameraPivot,
        float                         sensitivity,
        float                         minPitch,
        float                         maxPitch,
        float                         smoothTime,
        float                         rotationSmoothTime)
    {
        movementData                  = movementSO;
        MovementAnimationEventTrigger = movementAnimationEventTrigger;
        movementBrainStatePayload     = new MovementBrainStatePayload();

        movementMotor = new AdvancedCharacterControllerMotor(
            characterController,
            movementSO.GroundedData.GroundToFallRayDistance,
            movementSO.GroundedData.StickToGroundRayDistance,
            movementSO.GroundedData.GroundLayer,
            1,
            movementSO.AirborneData.Gravity.y);

        inputProvider             = new PlayerInputProvider(cameraPivot);
        movementStateMachine      = new MovementStateMachine(this);
        characterAnimatorController = new CharacterAnimatorController(
            animator, this, rotationSmoothTime, meshTransform);
        characterCamera           = new CharacterCamera(
            inputProvider, cameraPivot, sensitivity, minPitch, maxPitch, smoothTime);
    }

    // ── WorldSimulation registration ──────────────────────────────────────────

    public void Register(WorldSimulation simulation)   => simulation.Register(this);
    public void Unregister(WorldSimulation simulation) { if (simulation != null) simulation.Unregister(this); }

    // ── Tick entrypoints ──────────────────────────────────────────────────────

    /// <summary>Current live input from the local device.</summary>
    public PlayerInputPayload CurrentInputPayload => inputProvider.InputPayload;

    /// <summary>
    /// Per-frame update: polls input, advances animation, ticks camera.
    /// Called by the active wrapper's Unity Update().
    /// </summary>
    public void OnUpdate(float dt)
    {
        inputProvider.Tick(dt);
        characterAnimatorController.Tick(dt);
        characterCamera.Tick(dt);
    }

    // ── ISimulatableEntity ────────────────────────────────────────────────────

    void ISimulatableEntity.SimulateTick(float dt) => OnSimulateTick(dt);

    /// <summary>
    /// Override in subclasses to intercept the tick (e.g. to buffer input/state online).
    /// The base implementation uses live input.
    /// </summary>
    protected virtual void OnSimulateTick(float dt) => SimulateTick(dt, inputProvider.InputPayload);

    // ── ISimulatable ──────────────────────────────────────────────────────────

    /// <summary>
    /// Deterministic single-tick simulation.
    /// Must remain free of Time.time, Random, and any external mutable state.
    /// </summary>
    public PlayerStateSnapshot SimulateTick(float dt, PlayerInputPayload input)
    {
        movementStateMachine.Tick(dt, input);
        movementMotor.ApplyForce(dt, movementBrainStatePayload.IsGrounded);
        return TakeSnapshot();
    }

    /// <summary>
    /// Restores the simulation to a previous snapshot.
    /// Called during reconciliation replay before re-running buffered inputs.
    /// </summary>
    public void ApplyState(PlayerStateSnapshot state)
    {
        movementMotor.Position = state.Position;
        movementMotor.Velocity = state.Velocity;

        movementBrainStatePayload.ShouldWalk                = state.ShouldWalk;
        movementBrainStatePayload.ShouldSprint              = state.ShouldSprint;
        movementBrainStatePayload.MovementSpeedModifier     = state.MovementSpeedModifier;
        movementBrainStatePayload.MovementDecelerationForce = state.MovementDecelerationForce;
        movementBrainStatePayload.RemainingJump             = state.RemainingJump;
        movementBrainStatePayload.CurrentJumpForce          = state.CurrentJumpForce;
        movementBrainStatePayload.IsGrounded                = state.IsGrounded;
        movementBrainStatePayload.IsMoving                  = state.IsMoving;
        movementBrainStatePayload.IsStopping                = state.IsStopping;
        movementBrainStatePayload.IsLanding                 = state.IsLanding;
        movementBrainStatePayload.StateTimer                = state.StateTimer;

        IState targetState = MovementStateFromId(state.MovementStateId);
        if (movementStateMachine.CurrentState != targetState)
            movementStateMachine.ChangeState(targetState);
    }

    // ── Snapshot helpers ──────────────────────────────────────────────────────

    private PlayerStateSnapshot TakeSnapshot() => new()
    {
        Position                  = movementMotor.Position,
        Velocity                  = movementMotor.Velocity,
        MovementStateId           = MovementStateIdFromState(movementStateMachine.CurrentState),
        ShouldWalk                = movementBrainStatePayload.ShouldWalk,
        ShouldSprint              = movementBrainStatePayload.ShouldSprint,
        MovementSpeedModifier     = movementBrainStatePayload.MovementSpeedModifier,
        MovementDecelerationForce = movementBrainStatePayload.MovementDecelerationForce,
        RemainingJump             = movementBrainStatePayload.RemainingJump,
        CurrentJumpForce          = movementBrainStatePayload.CurrentJumpForce,
        IsGrounded                = movementBrainStatePayload.IsGrounded,
        IsMoving                  = movementBrainStatePayload.IsMoving,
        IsStopping                = movementBrainStatePayload.IsStopping,
        IsLanding                 = movementBrainStatePayload.IsLanding,
        StateTimer                = movementBrainStatePayload.StateTimer,
    };

    private MovementStateId MovementStateIdFromState(IState state)
    {
        if (state == movementStateMachine.IdlingState)         return MovementStateId.Idling;
        if (state == movementStateMachine.WalkingState)        return MovementStateId.Walking;
        if (state == movementStateMachine.RunningState)        return MovementStateId.Running;
        if (state == movementStateMachine.SprintingState)      return MovementStateId.Sprinting;
        if (state == movementStateMachine.LightStoppingState)  return MovementStateId.LightStopping;
        if (state == movementStateMachine.MediumStoppingState) return MovementStateId.MediumStopping;
        if (state == movementStateMachine.HardStoppingState)   return MovementStateId.HardStopping;
        if (state == movementStateMachine.LightLandingState)   return MovementStateId.LightLanding;
        if (state == movementStateMachine.HardLandingState)    return MovementStateId.HardLanding;
        if (state == movementStateMachine.RollingState)        return MovementStateId.Rolling;
        if (state == movementStateMachine.JumpingState)        return MovementStateId.Jumping;
        if (state == movementStateMachine.FallingState)        return MovementStateId.Falling;
        return MovementStateId.Idling;
    }

    private IState MovementStateFromId(MovementStateId id) => id switch
    {
        MovementStateId.Idling         => movementStateMachine.IdlingState,
        MovementStateId.Walking        => movementStateMachine.WalkingState,
        MovementStateId.Running        => movementStateMachine.RunningState,
        MovementStateId.Sprinting      => movementStateMachine.SprintingState,
        MovementStateId.LightStopping  => movementStateMachine.LightStoppingState,
        MovementStateId.MediumStopping => movementStateMachine.MediumStoppingState,
        MovementStateId.HardStopping   => movementStateMachine.HardStoppingState,
        MovementStateId.LightLanding   => movementStateMachine.LightLandingState,
        MovementStateId.HardLanding    => movementStateMachine.HardLandingState,
        MovementStateId.Rolling        => movementStateMachine.RollingState,
        MovementStateId.Jumping        => movementStateMachine.JumpingState,
        MovementStateId.Falling        => movementStateMachine.FallingState,
        _                              => movementStateMachine.IdlingState,
    };
}
