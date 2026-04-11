using System.Collections.Generic;
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

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Instance == this) Instance = null;
    }

    public void RegisterReconcilable(IReconcilableEntity entity)
    {
        if (!reconcilables.Contains(entity))
            reconcilables.Add(entity);
    }

    public void UnregisterReconcilable(IReconcilableEntity entity)
    {
        reconcilables.Remove(entity);
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
                Tick(Time.fixedDeltaTime);
        }

        currentTick++;
    }
}