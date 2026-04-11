/// <summary>
/// Contract that any brain must satisfy to be wrapped by ClientPrediction.
///
/// TInput  — the combined input struct for one simulation tick.
///           Must be a struct so the online layer can serialize it over the wire.
/// TState  — a full snapshot of the simulation state after the tick.
///           Must be a struct so the online layer can compare and serialize it.
///
/// Offline:  CharacterBrain.FixedUpdate() calls SimulateTick(dt, liveInput) and discards the result.
/// Online:   NetworkCharacterBrain passes the same call through ClientPrediction which
///           buffers inputs, buffers states, detects divergence, and calls SimulateTick
///           again during reconciliation replay — all without touching the brain internals.
/// </summary>
public interface ISimulatable<TInput, TState> : ISimulatableEntity
    where TInput : struct
    where TState : struct
{
    /// <summary>
    /// Advance the simulation by one tick using the given input.
    /// Must be purely deterministic: same (TState, TInput) always produces the same result.
    /// Must not read Time.time, Random, or any other non-deterministic source.
    /// Returns a full snapshot of the resulting state.
    /// </summary>
    TState SimulateTick(float dt, TInput input);

    /// <summary>
    /// Restore the simulation to a previously snapshotted state.
    /// Called by the online layer before replaying buffered inputs after a mismatch.
    /// </summary>
    void ApplyState(TState state);
}
