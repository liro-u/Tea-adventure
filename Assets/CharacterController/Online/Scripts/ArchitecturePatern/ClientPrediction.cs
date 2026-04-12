using System;
using Unity.Netcode;

/// <summary>
/// Generic client-side prediction layer for any simulatable entity.
///
/// Wraps an ISimulatable&lt;TInput, TState&gt; and adds:
///   - Input buffering       (one entry per tick, used during reconciliation replay)
///   - State buffering       (one snapshot per tick, compared against server corrections)
///   - Server input queue    (server consumes client inputs from this)
///   - Reconciliation        (IReconcilableEntity, driven by NetworkWorldSimulation)
///
/// This class IS the ISimulatableEntity registered with WorldSimulation — the underlying
/// ISimulatable is never registered directly online.
///
/// Transport is kept out: two Action callbacks are wired by the NetworkBehaviour wrapper
/// so this class has no NGO dependency beyond the NetworkBehaviour role reference.
///
/// Usage for a new networkable entity type:
///   1. Define TInput and TState structs (INetworkSerializable).
///   2. Implement ISimulatable&lt;TInput, TState&gt; on your core class.
///   3. Construct a ClientPrediction, set CheckDivergence and the two callbacks.
///   4. Register/unregister in OnNetworkSpawn / OnNetworkDespawn.
///   5. Wire EnqueueServerInput and ReceiveCorrection from your ServerRpc / ClientRpc.
/// </summary>
public class ClientPrediction<TInput, TState> : ISimulatableEntity, IReconcilableEntity
    where TInput : struct, INetworkSerializable
    where TState : struct, INetworkSerializable
{
    private const int BufferSize = 128;

    private struct StateSnapshot
    {
        public TState State;
        public int Tick;
    }

    private readonly ISimulatable<TInput, TState> simulatable;
    private readonly Func<TInput>                 getLiveInput;
    private readonly NetworkBehaviour             owner;

    private readonly TInput[] inputBuffer = new TInput[BufferSize];
    private readonly StateSnapshot[] stateBuffer = new StateSnapshot[BufferSize];

    // Ring buffer for server-side inputs indexed by tick.
    // EnqueueServerInput writes to buffer[tick % size]; SimulateTick reads at pendingTick.
    // Out-of-order or redundant writes are idempotent; missing inputs fall back to lastServerInput.
    private readonly TInput[] serverInputBuffer  = new TInput[BufferSize];
    private readonly bool[]   serverInputPresent = new bool[BufferSize];
    private TInput lastServerInput;

    private bool   hasPendingCorrection;
    private TState pendingCorrection;
    private int    pendingCorrectionTick;

    private int  pendingTick;
    private bool isReplaying;
    private int  replayTick;

    // ── Transport callbacks ───────────────────────────────────────────────────

    /// <summary>
    /// Called on the owning client each tick with (input, tick, prevInput, prevTick).
    /// Sends the current AND previous input so the server can recover from single packet loss.
    /// Wire to a ServerRpc in your NetworkBehaviour wrapper.
    /// </summary>
    public Action<TInput, int, TInput, int> OnSendInput;

    /// <summary>
    /// Called on the server each tick after simulating a remote player, with (state, tick).
    /// Wire to a targeted ClientRpc in your NetworkBehaviour wrapper.
    /// </summary>
    public Action<TState, int> OnSendStateCorrection;

    /// <summary>
    /// Returns true when a server correction diverges from the local prediction.
    /// Define the threshold per-feature. Example:
    ///   (server, local) => Vector3.Distance(server.Position, local.Position) > 0.001f
    /// </summary>
    public Func<TState, TState, bool> CheckDivergence;

    // ── Construction ──────────────────────────────────────────────────────────

    public ClientPrediction(
        ISimulatable<TInput, TState> simulatable,
        Func<TInput>                 getLiveInput,
        NetworkBehaviour             owner)
    {
        this.simulatable  = simulatable;
        this.getLiveInput = getLiveInput;
        this.owner        = owner;
    }

    // ── WorldSimulation registration helpers ──────────────────────────────────

    public void Register(WorldSimulation simulation)   => simulation.Register(this);
    public void Unregister(WorldSimulation simulation) { if (simulation != null) simulation.Unregister(this); }

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

    /// <summary>Server side: store an input received from the owning client, indexed by its tick.
    /// Safe to call multiple times for the same tick (redundant sends are idempotent).</summary>
    public void EnqueueServerInput(TInput input, int tick)
    {
        var slot = tick % BufferSize;
        serverInputBuffer[slot]  = input;
        serverInputPresent[slot] = true;
    }

    /// <summary>Owner client side: store an authoritative correction received from the server.</summary>
    public void ReceiveCorrection(TState state, int tick)
    {
        pendingCorrection     = state;
        pendingCorrectionTick = tick;
        hasPendingCorrection  = true;
    }

    // ── ISimulatableEntity ────────────────────────────────────────────────────

    void ISimulatableEntity.SimulateTick(float dt)
    {
        if (!owner.IsOwner && !owner.IsServer) return;

        TInput input;

        if (isReplaying)
        {
            input = inputBuffer[replayTick % BufferSize];
            replayTick++;
        }
        else if (owner.IsOwner)
        {
            input = getLiveInput();
            inputBuffer[pendingTick % BufferSize] = input;

            // Pure clients (not host) send current + previous input each tick.
            // The previous input lets the server recover from single packet loss.
            if (!owner.IsServer)
            {
                var prevTick  = pendingTick > 0 ? pendingTick - 1 : 0;
                var prevInput = inputBuffer[prevTick % BufferSize];
                OnSendInput?.Invoke(input, pendingTick, prevInput, prevTick);
            }
        }
        else
        {
            // Server for a remote player: read the input whose tick matches the current simulation tick.
            // Fall back to the last known input if none arrived (packet loss / reorder).
            var slot = pendingTick % BufferSize;
            if (serverInputPresent[slot])
            {
                input                    = serverInputBuffer[slot];
                lastServerInput          = input;
                serverInputPresent[slot] = false;
            }
            else
            {
                input = lastServerInput;
            }
        }

        var snapshot = simulatable.SimulateTick(dt, input);

        if (!isReplaying)
        {
            stateBuffer[pendingTick % BufferSize] = new StateSnapshot { State = snapshot, Tick = pendingTick };

            if (owner.IsServer)
                OnSendStateCorrection?.Invoke(snapshot, pendingTick);

            pendingTick++;
        }
    }

    // ── IReconcilableEntity ───────────────────────────────────────────────────

    public void SaveState(int tick)
    {
        pendingTick = tick;
        isReplaying = false;
    }

    public void RestoreState(int tick)
    {
        simulatable.ApplyState(stateBuffer[tick % BufferSize].State);
        isReplaying = true;
        replayTick  = tick;
    }

    public bool NeedsReconciliation(out int fromTick)
    {
        if (!hasPendingCorrection || CheckDivergence == null || owner.IsServer) { 
            fromTick = 0; 
            return false; 
        }

        hasPendingCorrection = false;

        int bufferSlot = pendingCorrectionTick % BufferSize;
        // Check if the buffer slot still contains the state from the correction's tick
        if (stateBuffer[bufferSlot].Tick != pendingCorrectionTick)
        {
            // Slot has been overwritten with new data, discard this stale correction
            fromTick = 0;
            return false;
        }

        if (!CheckDivergence(pendingCorrection, stateBuffer[bufferSlot].State))
        {
            fromTick = 0;
            return false;
        }

        // Overwrite with the authoritative state so RestoreState has the correct base.
        stateBuffer[bufferSlot] = new StateSnapshot { State = pendingCorrection, Tick = pendingCorrectionTick };
        fromTick = pendingCorrectionTick;
        return true;
    }
}
