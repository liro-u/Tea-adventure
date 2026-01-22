using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface ITickPayload
{
    int Tick { get; set; }
}

public abstract class ClientPredictionNetworkBehaviour<TInput, TState> : NetworkBehaviour
    where TInput : struct, ITickPayload
    where TState : struct, ITickPayload
{
    // Tick system
    public const float TICK_RATE = 120f;
    public const int BUFFER_SIZE = 1024;

    public float tickDelta { get; private set; }
    public float tickTimer { get; private set; }
    public int currentTick { get; private set; }

    // Buffers
    protected TInput[] inputBuffer = new TInput[BUFFER_SIZE];
    protected TState[] networkStateBuffer = new TState[BUFFER_SIZE];

    // Latest server state for reconciliation
    protected TState latestServerNetworkState;
    public TState CurrentNetworkState { get; protected set; }
    protected TState lastReconciledNetworkState;

    // Server-side input queue
    private readonly Queue<TInput> serverInputQueue = new Queue<TInput>();


    private void Awake()
    {
        inputBuffer = new TInput[BUFFER_SIZE];
        networkStateBuffer = new TState[BUFFER_SIZE];
    }

    public override void OnNetworkSpawn()
    {
        tickDelta = 1f / TICK_RATE;
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!IsOwner && !IsServer)
            return;

        if (tickDelta <= 0f) throw new InvalidOperationException(
            $"{nameof(ClientPredictionNetworkBehaviour<TInput, TState>)}: tickDelta is {tickDelta}. " +
            "OnNetworkSpawn() was not called or base.OnNetworkSpawn() was skipped."
        );

        while (tickTimer >= tickDelta)
        {
            tickTimer -= tickDelta;
            Tick();
            currentTick++;
        }
    }

    public TState GetPrevNetworkState(int tick)
    {
        int prevIndex = (tick - 1 + BUFFER_SIZE) % BUFFER_SIZE;
        TState prevNetworkState = networkStateBuffer[prevIndex];
        return prevNetworkState;
    }

    private void Tick()
    {
        if (IsOwner)
        {
            Predict();
            Reconcile();
        }
        if (IsServer)
        {
            while (serverInputQueue.Count > 0)
            {
                TInput input = serverInputQueue.Dequeue();

                // simulate server-side
                TState state = Simulate(input);
                networkStateBuffer[input.Tick % BUFFER_SIZE] = state;

                // send authoritative state back to client
                latestServerNetworkState = state;
                SendNetworkStateClientRpc(state);
            }
        }
    }

    // ================= CLIENT =================

    private void Predict() {
        int index = currentTick % BUFFER_SIZE;

        TInput input = CreateInputPayload(currentTick);
        inputBuffer[index] = input;

        TState predicted = Simulate(input);
        networkStateBuffer[index] = predicted;

        ApplyNetworkState(predicted);

        SendInputServerRpc(input);
    }
    protected virtual void ApplyNetworkState(TState state) {
        CurrentNetworkState = state;
    }

    public TState GetCurrentNetworkState()
    {
        return IsOwner ? CurrentNetworkState : latestServerNetworkState;
    }

    protected void Reconcile()
    {
        if (latestServerNetworkState.Tick == 0 ||
            latestServerNetworkState.Tick == lastReconciledNetworkState.Tick)
            return;

        lastReconciledNetworkState = latestServerNetworkState;

        int index = latestServerNetworkState.Tick % BUFFER_SIZE;
        
        if (!ReconciliationNeeded(latestServerNetworkState, networkStateBuffer[index])) return;

        // Rewind
        networkStateBuffer[index] = latestServerNetworkState;
        ApplyNetworkState(latestServerNetworkState);

        // Replay
        int tick = latestServerNetworkState.Tick + 1;
        while (tick < currentTick)
        {
            int i = tick % BUFFER_SIZE;
            var predicted = Simulate(inputBuffer[i]);
            ApplyNetworkState(predicted);
            networkStateBuffer[i] = predicted;
            tick++;
        }
    }
    protected abstract bool ReconciliationNeeded(TState latestServerState, TState matchingClientState);

    // ================= SERVER =================

    [ServerRpc]
    private void SendInputServerRpc(TInput input)
    {
        serverInputQueue.Enqueue(input);
    }

    [ClientRpc]
    private void SendNetworkStateClientRpc(TState state)
    {
        if (state.Tick <= latestServerNetworkState.Tick)
            return;

        latestServerNetworkState = state;
    }

    // ================= SHARED =================
    protected abstract TInput CreateInputPayload(int currentTick);

    protected abstract TState Simulate(TInput input);
}
