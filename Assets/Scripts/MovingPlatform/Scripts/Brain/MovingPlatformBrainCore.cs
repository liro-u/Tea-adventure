using UnityEngine;

/// <summary>
/// Pure C# brain for a moving platform — no MonoBehaviour, no NetworkBehaviour.
///
/// Implements both ISimulatableEntity (offline registration with WorldSimulation) and
/// ISimulatable&lt;TInput, TState&gt; (online wrapping by ClientPrediction) so the same
/// core works in both contexts without modification.
///
/// Replay suppression: SetReplayMode(true) before a reconciliation replay suppresses
/// EventZone physics callbacks, preventing re-triggers during replay ticks.
/// The future NetworkMovingPlatformBrain is responsible for calling this around replays.
///
/// Fixed tick flow:
///   WorldSimulation → ISimulatableEntity.SimulateTick(dt) → OnSimulateTick(dt)
///   OnSimulateTick polls the input provider then calls SimulateTick(dt, input).
///   NetworkMovingPlatformBrain (future) overrides OnSimulateTick to buffer and replay.
/// </summary>
public class MovingPlatformBrainCore : ISimulatableEntity, IMovingPlatformBrain,
    ISimulatable<MovingPlatformInputPayload, MovingPlatformStateSnapshot>
{
    // ── IMovingPlatformBrain ──────────────────────────────────────────────────

    public MovingPlatformSO                 PlatformData { get; }
    public IMovingPlatformBrainStatePayload StatePayload { get; set; }
    public MovingPlatformMotor              Motor        { get; }
    public Transform[]                      Waypoints    { get; }

    private readonly MovingPlatformInputProvider  inputProvider;
    private readonly MovingPlatformStateMachine   stateMachine;

    // ── Construction ──────────────────────────────────────────────────────────

    public MovingPlatformBrainCore(
        Transform        platformTransform,
        MovingPlatformSO platformData,
        Transform[]      waypoints,
        EventZone        eventZone)
    {
        PlatformData  = platformData;
        Waypoints     = waypoints;
        StatePayload  = new MovingPlatformBrainStatePayload();
        Motor         = new MovingPlatformMotor(platformTransform);
        inputProvider = new MovingPlatformInputProvider(eventZone);
        stateMachine  = new MovingPlatformStateMachine(this);
    }

    // ── WorldSimulation registration ──────────────────────────────────────────

    public void Register(WorldSimulation simulation)   => simulation.Register(this);
    public void Unregister(WorldSimulation simulation) { if (simulation != null) simulation.Unregister(this); }

    // ── Replay suppression hook ───────────────────────────────────────────────

    /// <summary>
    /// Suppresses EventZone physics callbacks while true.
    /// Call SetReplayMode(true) before replay starts, SetReplayMode(false) when it ends.
    /// Wired by the future NetworkMovingPlatformBrain around ClientPrediction replays.
    /// </summary>
    public void SetReplayMode(bool isReplaying) => inputProvider.Zone.IsReplayActive = isReplaying;

    // ── ISimulatableEntity ────────────────────────────────────────────────────

    void ISimulatableEntity.SimulateTick(float dt) => OnSimulateTick(dt);

    /// <summary>
    /// Override in a network subclass to intercept the tick for buffering and replay.
    /// The base implementation uses live input.
    /// </summary>
    protected virtual void OnSimulateTick(float dt)
    {
        inputProvider.Tick(dt);
        SimulateTick(dt, inputProvider.InputPayload);
    }

    // ── ISimulatable ──────────────────────────────────────────────────────────

    public MovingPlatformStateSnapshot SimulateTick(float dt, MovingPlatformInputPayload input)
    {
        stateMachine.Tick(dt, input);
        return TakeSnapshot();
    }

    public void ApplyState(MovingPlatformStateSnapshot state)
    {
        Motor.Position                      = state.Position;
        StatePayload.TargetWaypointIndex    = state.TargetWaypointIndex;
        StatePayload.WaypointDirection      = state.WaypointDirection;
        StatePayload.WaitTimer              = state.WaitTimer;
        StatePayload.IsActivated            = state.IsActivated;

        var targetState = StateFromId(state.StateId);
        if (stateMachine.CurrentState != targetState)
            stateMachine.ChangeState(targetState);
    }

    // ── Snapshot helpers ──────────────────────────────────────────────────────

    private MovingPlatformStateSnapshot TakeSnapshot() => new()
    {
        Position            = Motor.Position,
        TargetWaypointIndex = StatePayload.TargetWaypointIndex,
        WaypointDirection   = StatePayload.WaypointDirection,
        WaitTimer           = StatePayload.WaitTimer,
        IsActivated         = StatePayload.IsActivated,
        StateId             = StateIdFromState(stateMachine.CurrentState),
    };

    private MovingPlatformStateId StateIdFromState(IState state)
    {
        if (state == stateMachine.IdleState)    return MovingPlatformStateId.Idle;
        if (state == stateMachine.MovingState)  return MovingPlatformStateId.Moving;
        if (state == stateMachine.WaitingState) return MovingPlatformStateId.Waiting;
        return MovingPlatformStateId.Idle;
    }

    private IState StateFromId(MovingPlatformStateId id) => id switch
    {
        MovingPlatformStateId.Idle    => stateMachine.IdleState,
        MovingPlatformStateId.Moving  => stateMachine.MovingState,
        MovingPlatformStateId.Waiting => stateMachine.WaitingState,
        _                             => stateMachine.IdleState,
    };
}
