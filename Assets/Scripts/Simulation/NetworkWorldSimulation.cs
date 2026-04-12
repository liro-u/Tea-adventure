using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Extends WorldSimulation with client prediction reconciliation.
///
/// Each fixed tick:
///   1. All reconcilable entities snapshot their state (SaveState)
///   2. All entities tick normally (same Tick() call as offline)
///   3. Entities are asked whether a received server state diverges (NeedsReconciliation)
///   4. If any mismatch: rewind all entities to the earliest diverged tick, then
///      replay every tick up to the present using the same Tick() call —
///      entities serve their own buffered inputs internally during replay
/// </summary>
public class NetworkWorldSimulation : WorldSimulation
{
    public static new NetworkWorldSimulation Instance { get; private set; }

    private readonly List<IReconcilableEntity> reconcilables = new();
    private int currentTick;

    public int CurrentTick => currentTick;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted             += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback   += OnClientConnected;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Instance == this) Instance = null;
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted           -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    /// <summary>
    /// Reset currentTick when the server session starts so it aligns with NetworkManager.ServerTime.
    /// Without this, FixedUpdates that ran during the menu/lobby before StartHost() is called
    /// would accumulate in currentTick, making it diverge from ServerTime.Time / fixedDeltaTime.
    /// </summary>
    private void OnServerStarted() => currentTick = 0;

    /// <summary>
    /// When the local client connects, align currentTick to the server's elapsed time
    /// (plus RTT/2 via LocalTime) so corrections and buffer indices use the same tick numbers.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (!nm.IsServer && nm.LocalClientId == clientId)
            currentTick = Mathf.RoundToInt((float)nm.LocalTime.Time / Time.fixedDeltaTime);
    }

    public void RegisterReconcilable(IReconcilableEntity entity)
    {
        if (!reconcilables.Contains(entity))
        {
            reconcilables.Add(entity);
            // Align the entity's tick immediately so corrections arriving before the
            // first FixedUpdate (NGO-buffered RPCs delivered at spawn) are assessed
            // against a valid pendingTick rather than 0.
            entity.SaveState(currentTick);
        }
    }

    public void UnregisterReconcilable(IReconcilableEntity entity)
    {
        reconcilables.Remove(entity);
    }

    private void ReplayTick(float dt)
    {
        foreach (var e in reconcilables)
            e.SimulateTick(dt);
    }

    protected override void FixedUpdate()
    {
        foreach (var e in reconcilables)
            e.SaveState(currentTick);

        Tick(Time.fixedDeltaTime);

        int rewindTick = int.MaxValue;
        bool anyMismatch = false;

        foreach (var e in reconcilables)
        {
            if (e.NeedsReconciliation(out int fromTick) && fromTick < rewindTick)
            {
                anyMismatch = true;
                rewindTick = fromTick;
            }
        }

        if (anyMismatch)
        {
            foreach (var e in reconcilables)
                e.RestoreState(rewindTick);

            for (int t = rewindTick; t < currentTick; t++)
                ReplayTick(Time.fixedDeltaTime);
        }

        currentTick++;
    }
}