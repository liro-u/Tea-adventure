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

    [Tooltip("PingPong: reverses at each end. OneShot: stops at the last knot.\n" +
             "For continuous looping, leave this at OneShot and enable Closed on the SplineContainer instead.")]
    [field: SerializeField] public PlatformLoopMode LoopMode     { get; private set; } = PlatformLoopMode.PingPong;
}

public enum PlatformLoopMode
{
    /// <summary>Platform stops at the last knot and stays there.</summary>
    OneShot,
    /// <summary>Platform reverses direction at each end, bouncing back and forth.</summary>
    PingPong,
}
