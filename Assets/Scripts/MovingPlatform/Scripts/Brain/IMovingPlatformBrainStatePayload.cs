/// <summary>
/// Mutable per-tick state written by platform states and read by the brain/motor.
/// Keeping this behind an interface lets states stay decoupled from the concrete payload class.
/// </summary>
public interface IMovingPlatformBrainStatePayload
{
    /// <summary>Index into Waypoints[] the platform is currently moving toward.</summary>
    int   TargetWaypointIndex { get; set; }

    /// <summary>Remaining seconds to wait at a waypoint. Only active in WaitingState.</summary>
    float WaitTimer           { get; set; }

    /// <summary>True once the EventZone has been triggered and the platform has started moving.</summary>
    bool  IsActivated         { get; set; }

    /// <summary>+1 when traversing forward through waypoints, -1 when reversing (PingPong mode).</summary>
    int   WaypointDirection   { get; set; }
}
