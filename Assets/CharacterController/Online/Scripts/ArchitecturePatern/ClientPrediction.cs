using System.Collections.Generic;
using Unity.Netcode;

/// <summary>
/// Generic client prediction base for any network-controlled entity.
///
/// Each instance runs in one of four roles depending on which machine it lives on:
///
///   Host-owner   (IsOwner + IsServer)  — The server IS authoritative. Capture input,
///                                        simulate once, broadcast. No separate prediction.
///   Pure client  (IsOwner only)        — Predict locally to hide latency, send input to
///                                        server, reconcile when the server responds.
///   Server       (IsServer only)       — Drain the received input queue, simulate each
///                                        input authoritatively, broadcast state + input.
///   Spectator    (neither)             — Extrapolate from the last forwarded input so the
///                                        remote player looks smooth without extra round-trips.
/// </summary>
public abstract class ClientPrediction<TInput, TState> : ReplayableEntity<TState>
    where TInput : struct, ITickPayload
    where TState : struct, ITickPayload
{
    protected readonly NetworkBehaviour network;
    protected TInput[] inputBuffer = new TInput[BUFFER_SIZE];

    private readonly Queue<TInput> serverInputQueue = new();

    private TInput lastForwardedInput;
    private bool hasForwardedInput;

    private TState latestServerState;
    private TState lastReconciledState;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected ClientPrediction(NetworkBehaviour network)
    {
        this.network = network;
        ReplayManager.Instance.Register(this);
    }

    public void Dispose() => ReplayManager.Instance.Unregister(this);

    // ── IReplayable ──────────────────────────────────────────────────────────

    /// <summary>
    /// Owners always participate. The server entity participates only when input is queued.
    /// Spectators participate once they have received at least one forwarded input.
    /// </summary>
    public override bool CanSimulate
    {
        get
        {
            if (network.IsOwner) return true;
            if (network.IsServer) return serverInputQueue.Count > 0;
            return hasForwardedInput;
        }
    }

    public override void OnTick(int tick)
    {
        if (network.IsOwner && network.IsServer)
            TickHostOwner(tick);
        else if (network.IsOwner)
            TickPureClient(tick);
        else if (network.IsServer)
            TickServer();
        else
            TickSpectator(tick);
    }

    /// <summary>
    /// Replays one historical tick using the buffered input.
    /// Called by ReplayManager during reconciliation to re-simulate ticks after a mismatch.
    /// </summary>
    public override void SimulateTick(int tick)
    {
        int i = tick % BUFFER_SIZE;
        TState state = Simulate(inputBuffer[i]);
        stateBuffer[i] = state;
        ApplyState(state);
    }

    // ── Roles ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Host-owner: the server simulation IS the result — no prediction round-trip needed.
    /// Captures input, simulates once authoritatively, broadcasts state and forwards input
    /// to spectator clients.
    /// </summary>
    private void TickHostOwner(int tick)
    {
        int i = tick % BUFFER_SIZE;
        TInput input = CaptureLocalInput(tick);
        inputBuffer[i] = input;
        TState state = Simulate(input);
        stateBuffer[i] = state;
        ApplyState(state);
        BroadcastStateToClients(state);
        ForwardInputToClients(input);
    }

    /// <summary>
    /// Pure client: simulate immediately to hide latency, then send input to the server.
    /// Reconcile each tick once the server's authoritative response arrives.
    /// </summary>
    private void TickPureClient(int tick)
    {
        int i = tick % BUFFER_SIZE;
        TInput input = CaptureLocalInput(tick);
        inputBuffer[i] = input;
        TState predicted = Simulate(input);
        stateBuffer[i] = predicted;
        ApplyState(predicted);
        SendInputToServer(input);
        TryReconcile();
    }

    /// <summary>
    /// Server (remote player): drain all queued inputs, simulate each one, broadcast the
    /// resulting state, and forward input so spectator clients can extrapolate.
    /// </summary>
    private void TickServer()
    {
        while (serverInputQueue.Count > 0)
        {
            TInput input = serverInputQueue.Dequeue();
            int i = input.Tick % BUFFER_SIZE;
            TState state = Simulate(input);
            stateBuffer[i] = state;
            ApplyState(state);
            BroadcastStateToClients(state);
            ForwardInputToClients(input);
        }
    }

    /// <summary>
    /// Spectator (non-owner client): repeat the last known input each tick to approximate
    /// the remote player continuing their previous action, then reconcile against server state.
    /// </summary>
    private void TickSpectator(int tick)
    {
        int i = tick % BUFFER_SIZE;
        TInput input = WithTick(lastForwardedInput, tick);
        inputBuffer[i] = input;
        TState state = Simulate(input);
        stateBuffer[i] = state;
        ApplyState(state);
        TryReconcile();
    }

    // ── Reconciliation ───────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the latest server state diverges from the local prediction.
    /// If so, overwrites the mispredicted state, applies it to the world, and asks
    /// ReplayManager to re-simulate all subsequent ticks from the corrected base.
    /// Called by both owner (TickPureClient) and spectator (TickSpectator).
    /// </summary>
    private void TryReconcile()
    {
        if (latestServerState.Tick == 0 || latestServerState.Tick == lastReconciledState.Tick)
            return;

        lastReconciledState = latestServerState;

        int i = latestServerState.Tick % BUFFER_SIZE;
        if (!ReconciliationNeeded(latestServerState, stateBuffer[i])) return;

        stateBuffer[i] = latestServerState;
        ApplyState(latestServerState);
        ReplayManager.Instance.MarkDirty(latestServerState.Tick + 1);
    }

    // ── Receiving from network ────────────────────────────────────────────────

    public void ReceiveInputOnServer(TInput input) => serverInputQueue.Enqueue(input);

    public void ReceiveStateOnClient(TState state)
    {
        if (state.Tick > latestServerState.Tick)
            latestServerState = state;
    }

    public void ReceiveForwardedInput(TInput input)
    {
        if (input.Tick > lastForwardedInput.Tick)
        {
            lastForwardedInput = input;
            hasForwardedInput = true;
        }
    }

    // ── Abstract interface ────────────────────────────────────────────────────

    /// <summary>Poll local input and build the payload for this tick.</summary>
    protected abstract TInput CaptureLocalInput(int tick);

    /// <summary>
    /// Run one simulation step with the given input and return the resulting state.
    /// Must produce identical output on client, server, and during replay.
    /// </summary>
    protected abstract TState Simulate(TInput input);

    /// <summary>Returns true when the server state diverges enough to warrant a replay.</summary>
    protected abstract bool ReconciliationNeeded(TState serverState, TState clientState);

    protected abstract void SendInputToServer(TInput input);
    protected abstract void BroadcastStateToClients(TState state);
    protected abstract void ForwardInputToClients(TInput input);

    protected override void ApplyState(TState state) { }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TInput WithTick(TInput input, int tick)
    {
        input.Tick = tick;
        return input;
    }
}
