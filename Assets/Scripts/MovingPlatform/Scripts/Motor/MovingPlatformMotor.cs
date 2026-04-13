using UnityEngine;

/// <summary>
/// Executes transform movement for a moving platform. Pure C# — no MonoBehaviour.
///
/// Designed for deterministic per-tick simulation:
///   - MoveTowards advances position by at most Speed * dt each tick.
///   - Velocity is derived from the per-tick delta so character controllers
///     riding the platform can read it and apply the same offset.
/// </summary>
public class MovingPlatformMotor
{
    private readonly Transform transform;

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    /// <summary>
    /// World-space velocity of the platform during the last tick (delta / dt).
    /// Useful for character controllers that need to inherit platform movement.
    /// </summary>
    public Vector3 Velocity { get; private set; }

    public MovingPlatformMotor(Transform transform)
    {
        this.transform = transform;
    }

    /// <summary>
    /// Moves toward <paramref name="target"/> at <paramref name="speed"/> units/second.
    /// Returns true when the target position is reached exactly.
    /// </summary>
    public bool MoveTowards(Vector3 target, float speed, float dt)
    {
        var previous = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * dt);
        Velocity = (transform.position - previous) / dt;
        return transform.position == target;
    }

    public void ResetVelocity() => Velocity = Vector3.zero;
}
