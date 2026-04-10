using UnityEngine;

/// <summary>
/// Concrete client prediction for player movement.
/// Bridges NetworkCharacterBrain (networking) ↔ the movement simulation (motor + state machine).
///
/// Simulation always runs through the same path:
///   1. InjectInput()  — pushes the payload into remoteInput so the state machine reads it.
///   2. StateMachine.Tick + Motor.ApplyForce  — advances one simulation step.
///   3. CaptureState() — snapshots position and brain state for reconciliation.
/// </summary>
public class MovementClientPrediction : ClientPrediction<NetworkMovementInputPayload, NetworkMovementBrainStatePayload>
{
    private readonly NetworkCharacterBrain brain;
    private readonly NetworkMovementStateMachine networkStateMachine;

    public MovementClientPrediction(NetworkCharacterBrain brain) : base(brain)
    {
        this.brain = brain;
        networkStateMachine = (NetworkMovementStateMachine)brain.movementStateMachine;
    }

    // ── Simulation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Polls fresh local input and packages it as a network payload.
    /// The same values will be injected back via InjectInput() before Simulate() runs,
    /// so the captured payload and the simulated input are always identical.
    /// </summary>
    protected override NetworkMovementInputPayload CaptureLocalInput(int tick)
    {
        brain.PollLocalInput(TickDelta);
        IMovementInputPayload raw = brain.LocalInputPayload;
        return new NetworkMovementInputPayload
        {
            Tick        = tick,
            MoveInput   = raw.MoveInput,
            LookInput   = raw.LookInput,
            CameraPivot = raw.CameraPivot,
            IsSprinting = raw.IsSprinting,
            IsWalkToggle = raw.IsWalkToggle,
            IsJumping   = raw.IsJumping,
        };
    }

    /// <summary>
    /// Injects input and advances one tick. Identical on client predict, server, and replay.
    /// </summary>
    protected override NetworkMovementBrainStatePayload Simulate(NetworkMovementInputPayload input)
    {
        brain.InjectInput(input);
        brain.movementStateMachine.Tick(TickDelta);
        brain.movementMotor.ApplyForce(TickDelta, brain.movementBrainStatePayload.IsGrounded);
        return CaptureState(input.Tick);
    }

    private NetworkMovementBrainStatePayload CaptureState(int tick)
    {
        var s = (NetworkMovementBrainStatePayload)brain.movementBrainStatePayload;
        return new NetworkMovementBrainStatePayload
        {
            Tick                    = tick,
            Position                = brain.movementMotor.Position,
            StateId                 = networkStateMachine.getIdByState((MovementState)networkStateMachine.CurrentState),
            ShouldWalk              = s.ShouldWalk,
            ShouldSprint            = s.ShouldSprint,
            MovementSpeedModifier   = s.MovementSpeedModifier,
            MovementDecelerationForce = s.MovementDecelerationForce,
            RemainingJump           = s.RemainingJump,
            CurrentJumpForce        = s.CurrentJumpForce,
            IsGrounded              = brain.movementMotor.IsGrounded,
            IsMoving                = s.IsMoving,
            IsStopping              = s.IsStopping,
            IsLanding               = s.IsLanding,
        };
    }

    /// <summary>
    /// Restores position, brain state, and state machine to match the given snapshot.
    ///
    /// Uses direct CurrentState assignment instead of ChangeState() to avoid triggering
    /// Enter/Exit callbacks. This is intentional:
    ///   - After a normal simulation step the state machine is already in the right state,
    ///     so the assignment is a no-op.
    ///   - During reconciliation, the full payload is restored first (position, jump force,
    ///     remaining jumps, etc.), so the state's Tick() has everything it needs without
    ///     Enter() re-running setup that could double-decrement jumps or reset velocity.
    /// </summary>
    protected override void ApplyState(NetworkMovementBrainStatePayload state)
    {
        brain.movementMotor.Position = state.Position;
        brain.movementBrainStatePayload = state;
        brain.movementStateMachine.CurrentState = networkStateMachine.getStateById(state.StateId);
    }

    protected override bool ReconciliationNeeded(
        NetworkMovementBrainStatePayload serverState,
        NetworkMovementBrainStatePayload clientState)
    {
        return Vector3.Distance(serverState.Position, clientState.Position) > 0.001f;
    }

    // ── RPC bridges ───────────────────────────────────────────────────────────

    protected override void SendInputToServer(NetworkMovementInputPayload input)
        => brain.SendInputServerRpc(input);

    protected override void BroadcastStateToClients(NetworkMovementBrainStatePayload state)
        => brain.BroadcastStateClientRpc(state);

    protected override void ForwardInputToClients(NetworkMovementInputPayload input)
        => brain.ForwardInputClientRpc(input);
}
