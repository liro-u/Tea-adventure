/// <summary>
/// Extends ISimulatableEntity with the reconciliation contract required by NetworkWorldSimulation.
///
/// Each entity that participates in client prediction must implement this interface.
/// The entity owns its input buffer and state buffer — the world manager only orchestrates
/// when to save, restore, and check for mismatches.
///
/// Reconciliation flow (driven by NetworkWorldSimulation):
///   1. SaveState(tick)          — called before each tick so state[tick] is available for rewind
///   2. NeedsReconciliation()    — called after tick; entity checks received server states
///   3. RestoreState(tick)       — rewind all entities to the earliest mismatch tick
///   4. WorldSimulation.Tick()   — replayed for every tick from rewind to present;
///                                 entity serves buffered input internally (no extra call needed)
/// </summary>
public interface IReconcilableEntity : ISimulatableEntity
{
    /// <summary>
    /// Snapshot the current simulation state at the given tick index.
    /// Called by NetworkWorldSimulation before each Tick() so the state is available for rewind.
    /// </summary>
    void SaveState(int tick);

    /// <summary>
    /// Restore the simulation to the snapshotted state at the given tick index.
    /// After this call the entity must serve buffered input for ticks >= tick
    /// so that subsequent Tick() calls during replay are deterministic.
    /// </summary>
    void RestoreState(int tick);

    /// <summary>
    /// Returns true if a received server state diverges from the local prediction.
    /// fromTick is the earliest tick at which the divergence was detected.
    /// The world manager takes the minimum fromTick across all entities as the rewind point.
    /// </summary>
    bool NeedsReconciliation(out int fromTick);
}