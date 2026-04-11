/// <summary>
/// Non-generic simulation contract used by WorldSimulation to drive any entity
/// without knowing its input or state types.
///
/// Offline:  WorldSimulation.FixedUpdate() calls SimulateTick(dt) on every registered entity.
/// Online:   NetworkWorldSimulation does the same, but wraps the loop with rewind/replay
///           when any entity reports a reconciliation mismatch.
///
/// Entities pull their own input inside SimulateTick — the world manager never touches input.
/// </summary>
public interface ISimulatableEntity
{
    /// <summary>
    /// Advance the entity by one fixed tick.
    /// The entity is responsible for sourcing its own input (live or buffered).
    /// </summary>
    void SimulateTick(float dt);
}