using UnityEngine;

/// <summary>
/// Online wrapper around CharacterBrain.
/// Inherits the full offline simulation unchanged and adds:
///   - Input buffering  (one entry per tick, used during reconciliation replay)
///   - State buffering  (one snapshot per tick, compared against server corrections)
///   - IReconcilableEntity so NetworkWorldSimulation can rewind + replay this entity
///
/// Offline tick flow is untouched — OnSimulateTick is overridden to buffer before
/// and after the base simulation call, nothing else changes.
///
/// TODO (when NGO transport is wired up):
///   - Swap inputProvider for a NetworkInputProvider that receives server-relayed inputs
///   - Implement NeedsReconciliation to compare stateBuffer against received server snapshots
///   - Add INetworkSerializable to PlayerStateSnapshot and PlayerInputPayload
/// </summary>
public class NetworkCharacterBrain : CharacterBrain, IReconcilableEntity
{
    private const int BufferSize = 128;

    private readonly PlayerInputPayload[]    inputBuffer = new PlayerInputPayload[BufferSize];
    private readonly PlayerStateSnapshot[]   stateBuffer = new PlayerStateSnapshot[BufferSize];

    // Tick index at which SaveState was last called — used to index the buffers in OnSimulateTick.
    private int pendingTick;

    // During reconciliation replay the entity must serve buffered inputs instead of live input.
    // replayFromTick is set by RestoreState; OnSimulateTick increments it each replay call.
    private bool  isReplaying;
    private int   replayTick;

    protected override void Awake()
    {
        base.Awake();
        NetworkWorldSimulation.Instance.RegisterReconcilable(this);
    }

    private new void OnDestroy()
    {
        NetworkWorldSimulation.Instance?.UnregisterReconcilable(this);
    }

    // ── IReconcilableEntity ───────────────────────────────────────────────────

    public void SaveState(int tick)
    {
        pendingTick = tick;
        isReplaying = false;
    }

    public void RestoreState(int tick)
    {
        ApplyState(stateBuffer[tick % BufferSize]);
        isReplaying  = true;
        replayTick   = tick;
    }

    public bool NeedsReconciliation(out int fromTick)
    {
        // TODO: compare stateBuffer entries against server-authoritative snapshots received via NGO.
        // Return true and set fromTick to the earliest diverging tick.
        fromTick = 0;
        return false;
    }

    // ── Simulation hook ───────────────────────────────────────────────────────

    protected override void OnSimulateTick(float dt)
    {
        PlayerInputPayload input;

        if (isReplaying)
        {
            input = inputBuffer[replayTick % BufferSize];
            replayTick++;
        }
        else
        {
            input = inputProvider.InputPayload;
            inputBuffer[pendingTick % BufferSize] = input;
        }

        stateBuffer[pendingTick % BufferSize] = SimulateTick(dt, input);

        if (!isReplaying)
            pendingTick++;
    }
}