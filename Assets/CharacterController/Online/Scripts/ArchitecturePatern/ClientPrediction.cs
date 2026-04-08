using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface ITickPayload
{
    int Tick { get; set; }
}

public abstract class ClientPrediction<TInput, TState>
    where TInput : struct, ITickPayload
    where TState : struct, ITickPayload
{
    public NetworkBehaviour NetworkBehaviour { get; protected set; }

    public float TICK_RATE { get; protected set; }
    public const int BUFFER_SIZE = 1024;

    public float tickDelta { get; private set; }
    public float tickTimer { get; private set; }
    public int currentTick { get; private set; }

    protected TInput[] inputBuffer = new TInput[BUFFER_SIZE];
    protected TState[] networkStateBuffer = new TState[BUFFER_SIZE];

    protected TState latestServerNetworkState;
    protected TInput latestRemoteInput;
    protected bool hasRemoteInput;
    public TState CurrentNetworkState { get; protected set; }
    protected TState lastReconciledNetworkState;

    private readonly Queue<TInput> serverInputQueue = new Queue<TInput>();

    public ClientPrediction(NetworkBehaviour networkBehaviour, float tickRate = 50f)
    {
        NetworkBehaviour = networkBehaviour;
        TICK_RATE = tickRate;

        inputBuffer = new TInput[BUFFER_SIZE];
        networkStateBuffer = new TState[BUFFER_SIZE];

        tickDelta = 1f / TICK_RATE;
    }

    public void Update()
    {
        tickTimer += Time.deltaTime;

        if (!NetworkBehaviour.IsClient && !NetworkBehaviour.IsServer)
            return;

        if (tickDelta <= 0f) throw new InvalidOperationException(
            $"{nameof(ClientPrediction<TInput, TState>)}: tickDelta is {tickDelta}. " +
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
        return networkStateBuffer[prevIndex];
    }

    private void Tick()
    {
        if (NetworkBehaviour.IsClient)
        {
            Predict();
            Reconcile();
        }
        if (NetworkBehaviour.IsServer)
        {
            while (serverInputQueue.Count > 0)
            {
                TInput input = serverInputQueue.Dequeue();

                TState state = Simulate(input);
                networkStateBuffer[input.Tick % BUFFER_SIZE] = state;

                latestServerNetworkState = state;
                BroadcastStateToClients(state);
                ForwardInputToClients(input);
            }
        }
    }

    // ================= CLIENT =================

    private void Predict()
    {
        int index = currentTick % BUFFER_SIZE;

        TInput input;
        if (NetworkBehaviour.IsOwner)
        {
            input = CreateInputPayload(currentTick);
        }
        else
        {
            if (!hasRemoteInput)
                return;
            input = CreateExtrapolationInputPayload(currentTick, latestRemoteInput);
        }

        inputBuffer[index] = input;

        if (!(NetworkBehaviour.IsServer && NetworkBehaviour.IsOwner))
        {
            TState predicted = Simulate(input);
            networkStateBuffer[index] = predicted;
            ApplyNetworkState(predicted);
        }

        if (NetworkBehaviour.IsOwner)
            SendInputToServer(input);
    }

    protected virtual void ApplyNetworkState(TState state)
    {
        CurrentNetworkState = state;
    }

    public TState GetCurrentNetworkState()
    {
        return NetworkBehaviour.IsOwner && !NetworkBehaviour.IsServer
            ? CurrentNetworkState
            : latestServerNetworkState;
    }

    protected void Reconcile()
    {
        if (latestServerNetworkState.Tick == 0 ||
            latestServerNetworkState.Tick == lastReconciledNetworkState.Tick)
            return;

        lastReconciledNetworkState = latestServerNetworkState;

        int index = latestServerNetworkState.Tick % BUFFER_SIZE;

        if (!ReconciliationNeeded(latestServerNetworkState, networkStateBuffer[index])) return;

        networkStateBuffer[index] = latestServerNetworkState;
        ApplyNetworkState(latestServerNetworkState);

        int tick = latestServerNetworkState.Tick + 1;
        while (tick < currentTick)
        {
            if (!NetworkBehaviour.IsOwner && !hasRemoteInput)
                break;
            int i = tick % BUFFER_SIZE;
            var predicted = Simulate(inputBuffer[i]);
            ApplyNetworkState(predicted);
            networkStateBuffer[i] = predicted;
            tick++;
        }
    }

    protected abstract bool ReconciliationNeeded(TState latestServerState, TState matchingClientState);

    // ================= RPC BRIDGES =================
    // Implemented by the owning NetworkBehaviour — RPCs only work on NetworkBehaviour subclasses.

    protected abstract void SendInputToServer(TInput input);
    protected abstract void BroadcastStateToClients(TState state);
    protected abstract void ForwardInputToClients(TInput input);

    // ================= RECEIVE =================
    // Called by the NetworkBehaviour's [ServerRpc] / [ClientRpc] methods.

    public void OnReceiveInputFromClient(TInput input)
    {
        serverInputQueue.Enqueue(input);
    }

    public void OnReceiveStateFromServer(TState state)
    {
        if (state.Tick <= latestServerNetworkState.Tick)
            return;
        latestServerNetworkState = state;
    }

    public void OnReceiveForwardedInput(TInput input)
    {
        if (input.Tick <= latestRemoteInput.Tick)
            return;
        latestRemoteInput = input;
        hasRemoteInput = true;
    }

    // ================= SHARED =================

    protected abstract TInput CreateInputPayload(int currentTick);

    protected TInput CreateExtrapolationInputPayload(int currentTick, TInput latestRemoteInput)
    {
        TInput extrapolationInputPayload = latestRemoteInput;
        extrapolationInputPayload.Tick = currentTick;
        return extrapolationInputPayload;
    }

    protected abstract TState Simulate(TInput input);
}
