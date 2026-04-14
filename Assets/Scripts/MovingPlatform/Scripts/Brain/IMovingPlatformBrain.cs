using UnityEngine.Splines;

/// <summary>
/// Contract exposed to platform states.
/// Gives read access to data, motor, and spline path without coupling states to the concrete brain.
/// </summary>
public interface IMovingPlatformBrain
{
    MovingPlatformSO                 PlatformData { get; }
    IMovingPlatformBrainStatePayload StatePayload { get; set; }
    MovingPlatformMotor              Motor        { get; }
    SplineContainer                  SplinePath   { get; }
}
