using System;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Online extension of MovingPlatformBrainCore.
/// Adds input/state buffering, client-side prediction, and server reconciliation.
///
/// Unlike player-owned entities (where only the owner predicts), the moving platform
/// is a world entity: every client simulates it locally, buffers states, and reconciles
/// against server corrections. No ServerRpc is needed — the server generates its own
/// input from the server-side EventZone.
///
/// Server side:
///   Registered with WorldSimulation (simulation only, no reconciliation).
///   Each tick: ticks the input provider, simulates, fires OnSendStateCorrection.
///
/// Client side:
///   Registered with NetworkWorldSimulation for both simulation and reconciliation.
///   Each tick: ticks the local EventZone, buffers the input and resulting state.
///   On receiving a server correction: if it diverges from the local prediction,
///   RestoreState rewinds to the corrected tick and the reconciliation loop replays
///   all buffered inputs forward to rebuild the present.
///
/// Replay suppression:
///   SetReplayMode(true) is active during replay so EventZone physics callbacks
///   (OnTriggerEnter/Exit) are suppressed and cannot corrupt overlapCount while
///   replayed character movement re-enters the zone. SetReplayMode(false) is
///   restored in SaveState at the start of the next tick.
///
///   Note: EventZone.overlapCount is not part of the snapshot, so after a rewind
///   it may be approximate. In practice this is benign — the character is usually
///   already inside the zone before divergence occurs, and the next normal physics
///   frame restores the count correctly.
/// </summary>
public class NetworkMovingPlatformBrainCore : MovingPlatformBrainCore, IReconcilableEntity
{
    private const int BufferSize = 128;

    private struct StateEntry
    {
        public MovingPlatformStateSnapshot State;
        public int                         Tick;
    }

    private readonly MovingPlatformInputPayload[] inputBuffer = new MovingPlatformInputPayload[BufferSize];
    private readonly StateEntry[]                 stateBuffer = new StateEntry[BufferSize];

    private bool                       hasPendingCorrection;
    private MovingPlatformStateSnapshot pendingCorrection;
    private int                        pendingCorrectionTick;

    private int  pendingTick;
    private bool isReplaying;
    private int  replayTick;

    // ── Transport / divergence callbacks ─────────────────────────────────────

    /// <summary>
    /// Called on the server each tick after simulating, with (state, tick).
    /// Wire to a ClientRpc in NetworkMovingPlatformBrain. Leave null on clients.
    /// </summary>
    public Action<MovingPlatformStateSnapshot, int> OnSendStateCorrection;

    /// <summary>
    /// Returns true when a server correction diverges from the local prediction.
    /// Define the threshold per the feature's acceptable error. Example:
    ///   (server, local) => Vector3.Distance(server.Position, local.Position) > 0.01f
    /// </summary>
    public Func<MovingPlatformStateSnapshot, MovingPlatformStateSnapshot, bool> CheckDivergence;

    // ── Construction ──────────────────────────────────────────────────────────

    public NetworkMovingPlatformBrainCore(
        Transform        platformTransform,
        MovingPlatformSO platformData,
        SplineContainer  splinePath,
        EventZone        eventZone)
        : base(platformTransform, platformData, splinePath, eventZone)
    { }

    // ── Registration helpers ──────────────────────────────────────────────────

    /// <summary>Aligns pendingTick with the global simulation tick on the server at spawn.</summary>
    public void InitializeTick(int tick) => pendingTick = tick;

    /// <summary>Registers for both simulation and reconciliation on the client.</summary>
    public void RegisterWithReconciliation(NetworkWorldSimulation simulation)
    {
        simulation.Register(this);
        simulation.RegisterReconcilable(this);
    }

    public void UnregisterWithReconciliation(NetworkWorldSimulation simulation)
    {
        if (simulation == null) return;
        simulation.Unregister(this);
        simulation.UnregisterReconcilable(this);
    }

    // ── Called by the NetworkBehaviour RPC layer ──────────────────────────────

    /// <summary>Client side: store an authoritative correction received from the server.</summary>
    public void ReceiveCorrection(MovingPlatformStateSnapshot state, int tick)
    {
        pendingCorrection     = state;
        pendingCorrectionTick = tick;
        hasPendingCorrection  = true;
    }

    // ── ISimulatableEntity (override) ─────────────────────────────────────────

    protected override void OnSimulateTick(float dt)
    {
        MovingPlatformInputPayload input;

        if (isReplaying)
        {
            input = inputBuffer[replayTick % BufferSize];
            var snapshot = SimulateTick(dt, input);
            stateBuffer[replayTick % BufferSize] = new StateEntry { State = snapshot, Tick = replayTick };
            replayTick++;
        }
        else
        {
            inputProvider.Tick(dt);
            input = inputProvider.InputPayload;
            inputBuffer[pendingTick % BufferSize] = input;

            var snapshot = SimulateTick(dt, input);
            stateBuffer[pendingTick % BufferSize] = new StateEntry { State = snapshot, Tick = pendingTick };
            OnSendStateCorrection?.Invoke(snapshot, pendingTick);
            pendingTick++;
        }
    }

    // ── IReconcilableEntity ───────────────────────────────────────────────────

    public void SaveState(int tick)
    {
        pendingTick = tick;
        isReplaying = false;
        SetReplayMode(false);
    }

    public void RestoreState(int tick)
    {
        ApplyState(stateBuffer[tick % BufferSize].State);
        isReplaying = true;
        replayTick  = tick;
        SetReplayMode(true);
    }

    public bool NeedsReconciliation(out int fromTick)
    {
        if (!hasPendingCorrection || CheckDivergence == null)
        {
            fromTick = 0;
            return false;
        }

        hasPendingCorrection = false;

        int slot = pendingCorrectionTick % BufferSize;
        if (stateBuffer[slot].Tick != pendingCorrectionTick)
        {
            if (pendingTick - pendingCorrectionTick >= BufferSize)
                Debug.LogWarning($"[NetworkMovingPlatformBrainCore] Reconciliation skipped: correction tick {pendingCorrectionTick} is {pendingTick - pendingCorrectionTick} ticks behind (buffer size {BufferSize}). Consider increasing BufferSize or reducing RTT.");
            fromTick = 0;
            return false;
        }

        if (!CheckDivergence(pendingCorrection, stateBuffer[slot].State))
        {
            fromTick = 0;
            return false;
        }

        // Overwrite with the authoritative state so RestoreState has the correct base.
        stateBuffer[slot] = new StateEntry { State = pendingCorrection, Tick = pendingCorrectionTick };
        fromTick = pendingCorrectionTick;
        return true;
    }
}
