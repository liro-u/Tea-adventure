using UnityEngine;

/// <summary>
/// Tuning data for a moving platform. Numerical values only — no scene references.
/// </summary>
[CreateAssetMenu(fileName = "MovingPlatformSO", menuName = "Tea Adventure/Moving Platform")]
public class MovingPlatformSO : ScriptableObject
{
    [field: SerializeField] public float           Speed         { get; private set; } = 3f;

    [Tooltip("Time in seconds the platform pauses at each waypoint. 0 = no pause.")]
    [field: SerializeField] public float           WaitDuration  { get; private set; } = 0f;

    [Tooltip("PingPong: reverses at each end. Loop: wraps back to first waypoint. Neither: stops at last waypoint.")]
    [field: SerializeField] public PlatformLoopMode LoopMode     { get; private set; } = PlatformLoopMode.PingPong;
}

public enum PlatformLoopMode
{
    /// <summary>Platform stops at the last waypoint and stays there.</summary>
    OneShot,
    /// <summary>Platform reverses direction at each end, bouncing back and forth.</summary>
    PingPong,
    /// <summary>Platform wraps from the last waypoint back to the first.</summary>
    Loop,
}
