using UnityEngine;

/// <summary>
/// The player character's root MonoBehaviour.
/// Owns every sub-system, wires them together, and drives the tick loops.
///
/// Implements ISimulatable&lt;PlayerInputPayload, PlayerStateSnapshot&gt; so the
/// future NetworkCharacterBrain can wrap it inside ClientPrediction with zero
/// changes to this class: it only needs to call SimulateTick / ApplyState.
///
/// Offline tick flow (FixedUpdate):
///   inputProvider.Tick() → SimulateTick(dt, liveInput)
///     → movementStateMachine.Tick(dt, input)
///     → movementMotor.ApplyForce(dt)
///     → returns snapshot (discarded offline, used by ClientPrediction online)
///
/// Visual / Update-rate tick (Update):
///   characterAnimatorController.Tick()
///   characterCamera.Tick()
///   inputProvider.Tick()       ← polls device; never called by SimulateTick
/// </summary>
public class CharacterBrain : MonoBehaviour, IMovementBrain,
    ISimulatable<PlayerInputPayload, PlayerStateSnapshot>
{
    [SerializeField] public CharacterController characterController;
    [SerializeField] private MovementSO movementSO;
    [SerializeField] private MovementAnimationEventTrigger movementAnimationEventTrigger;

    [SerializeField] public Animator animator;
    [SerializeField] Transform meshTransform;
    [SerializeField] float rotationSmoothTime = 0.1f;

    [SerializeField] public Transform cameraPivot;
    [SerializeField] public float sensitivity = 2.5f;
    [SerializeField] public float minPitch = -40f;
    [SerializeField] public float maxPitch = 80f;
    [SerializeField] public float smoothTime = 0.05f;

    // ── IMovementBrain ────────────────────────────────────────────────────────

    public MovementSO movementData
    {
        get => movementSO;
        protected set => movementSO = value;
    }

    public IMovementBrainStatePayload movementBrainStatePayload { get; set; }
    public IAdvancedMotor movementMotor { get; protected set; }
    public MovementStateMachine movementStateMachine { get; protected set; }
    public CharacterAnimatorController characterAnimatorController { get; protected set; }
    public MovementAnimationEventTrigger MovementAnimationEventTrigger => movementAnimationEventTrigger;

    // ── Internal ──────────────────────────────────────────────────────────────

    protected IInputProvider<PlayerInputPayload> inputProvider;
    protected CharacterCamera characterCamera;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    public void Awake()
    {
        movementBrainStatePayload = new MovementBrainStatePayload();

        movementMotor = new AdvancedCharacterControllerMotor(
            characterController,
            movementSO.GroundedData.GroundToFallRayDistance,
            movementSO.GroundedData.StickToGroundRayDistance,
            movementSO.GroundedData.GroundLayer,
            1,
            movementSO.AirborneData.Gravity.y);

        inputProvider = new PlayerInputProvider(cameraPivot);

        movementStateMachine = new MovementStateMachine(this);

        characterAnimatorController = new CharacterAnimatorController(animator, this, rotationSmoothTime, meshTransform);

        characterCamera = new CharacterCamera(inputProvider, cameraPivot, sensitivity, minPitch, maxPitch, smoothTime);
    }

    public void Update()
    {
        inputProvider.Tick(Time.deltaTime);
        characterAnimatorController.Tick(Time.deltaTime);
        characterCamera.Tick(Time.deltaTime);
    }

    public void FixedUpdate()
    {
        SimulateTick(Time.fixedDeltaTime, inputProvider.InputPayload);
    }

    private void LateUpdate()
    {
        Debuger.Instance.Clear();
        Debuger.Instance.Add($"State    : {movementStateMachine.CurrentState}");
        Debuger.Instance.Add($"Position : {movementMotor.Position}");
        Debuger.Instance.Add($"Velocity : {movementMotor.Velocity}");
        Debuger.Instance.Add($"Grounded : {movementMotor.IsGrounded}");
    }

    // ── ISimulatable ──────────────────────────────────────────────────────────

    /// <summary>
    /// Advances the simulation by one fixed tick.
    /// This is the single entry point for both offline (called from FixedUpdate)
    /// and online (called by ClientPrediction during live ticks and reconciliation replay).
    /// Must remain deterministic — no Time.time, no Random inside.
    /// </summary>
    public PlayerStateSnapshot SimulateTick(float dt, PlayerInputPayload input)
    {
        movementStateMachine.Tick(dt, input);
        movementMotor.ApplyForce(dt, movementBrainStatePayload.IsGrounded);
        return TakeSnapshot();
    }

    /// <summary>
    /// Restores the simulation to a previous snapshot.
    /// Called by ClientPrediction before replaying buffered inputs after a mismatch.
    /// </summary>
    public void ApplyState(PlayerStateSnapshot state)
    {
        movementMotor.Position = state.Position;
        movementMotor.Velocity = state.Velocity;

        movementBrainStatePayload.ShouldWalk              = state.ShouldWalk;
        movementBrainStatePayload.ShouldSprint            = state.ShouldSprint;
        movementBrainStatePayload.MovementSpeedModifier   = state.MovementSpeedModifier;
        movementBrainStatePayload.MovementDecelerationForce = state.MovementDecelerationForce;
        movementBrainStatePayload.RemainingJump           = state.RemainingJump;
        movementBrainStatePayload.CurrentJumpForce        = state.CurrentJumpForce;
        movementBrainStatePayload.IsGrounded              = state.IsGrounded;
        movementBrainStatePayload.IsMoving                = state.IsMoving;
        movementBrainStatePayload.IsStopping              = state.IsStopping;
        movementBrainStatePayload.IsLanding               = state.IsLanding;
        movementBrainStatePayload.StateTimer              = state.StateTimer;

        IState targetState = MovementStateFromId(state.MovementStateId);
        if (movementStateMachine.CurrentState != targetState)
            movementStateMachine.ChangeState(targetState);
    }

    // ── Snapshot helpers ──────────────────────────────────────────────────────

    private PlayerStateSnapshot TakeSnapshot()
    {
        return new PlayerStateSnapshot
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
    }

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
