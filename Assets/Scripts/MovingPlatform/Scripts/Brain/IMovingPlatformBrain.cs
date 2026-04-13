using UnityEngine;

/// <summary>
/// Contract exposed to platform states.
/// Gives read access to data, motor, and waypoints without coupling states to the concrete brain.
/// </summary>
public interface IMovingPlatformBrain
{
    MovingPlatformSO                 PlatformData { get; }
    IMovingPlatformBrainStatePayload StatePayload { get; set; }
    MovingPlatformMotor              Motor        { get; }
    Transform[]                      Waypoints    { get; }
}
