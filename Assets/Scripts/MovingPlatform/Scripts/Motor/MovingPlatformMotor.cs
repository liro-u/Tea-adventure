using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Executes transform movement for a moving platform. Pure C# — no MonoBehaviour.
///
/// Designed for deterministic per-tick simulation:
///   - MoveAlongSpline advances position along a Bezier spline at constant world-space speed.
///   - Velocity is derived from the per-tick delta so character controllers
///     riding the platform can read it and apply the same offset.
///
/// Spline positions are stored in the SplineContainer's local space; all evaluation
/// is transformed to world space via the container's localToWorldMatrix.
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
    /// Advances <paramref name="t"/> along <paramref name="path"/> at a constant world-space speed,
    /// repositions the platform, and updates Velocity.
    ///
    /// Open spline: t is clamped to [0,1]; returns true when the boundary (0 or 1) is reached.
    /// Closed spline: t wraps with Mathf.Repeat; always returns false (no boundary).
    /// </summary>
    public bool MoveAlongSpline(SplineContainer path, ref float t, int direction, float speed, float dt)
    {
        var   spline = path.Spline;
        float length = SplineUtility.CalculateLength(spline, (float4x4)path.transform.localToWorldMatrix);
        if (length < 0.0001f) { Velocity = Vector3.zero; return false; }

        t += direction * speed * dt / length;

        bool hitBoundary;
        if (spline.Closed)
        {
            t            = Mathf.Repeat(t, 1f);
            hitBoundary  = false;
        }
        else
        {
            hitBoundary = direction > 0 ? t >= 1f : t <= 0f;
            t           = Mathf.Clamp01(t);
        }

        var previous = transform.position;
        SplineUtility.Evaluate(spline, t, out float3 localPos, out _, out _);
        transform.position = path.transform.TransformPoint((Vector3)localPos);
        Velocity = (transform.position - previous) / dt;
        return hitBoundary;
    }

    /// <summary>
    /// Snaps the platform to the given normalized spline position without affecting Velocity.
    /// </summary>
    public void PlaceOnSpline(SplineContainer path, float t)
    {
        SplineUtility.Evaluate(path.Spline, Mathf.Clamp01(t), out float3 localPos, out _, out _);
        transform.position = path.transform.TransformPoint((Vector3)localPos);
    }

    public void ResetVelocity() => Velocity = Vector3.zero;
}
