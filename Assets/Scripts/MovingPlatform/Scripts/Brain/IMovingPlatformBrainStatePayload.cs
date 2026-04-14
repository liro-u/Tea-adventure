/// <summary>
/// Mutable per-tick state written by platform states and read by the brain/motor.
/// Keeping this behind an interface lets states stay decoupled from the concrete payload class.
/// </summary>
public interface IMovingPlatformBrainStatePayload
{
    /// <summary>Normalized position along the spline (0 = first knot, 1 = last knot).</summary>
    float SplineT           { get; set; }

    /// <summary>Index of the spline knot the platform is currently heading toward.</summary>
    int   TargetKnotIndex   { get; set; }

    /// <summary>Remaining seconds to wait at a knot. Only active in WaitingState.</summary>
    float WaitTimer         { get; set; }

    /// <summary>True once the EventZone has been triggered and the platform has started moving.</summary>
    bool  IsActivated       { get; set; }

    /// <summary>+1 when traversing forward through knots, -1 when reversing (PingPong mode).</summary>
    int   WaypointDirection { get; set; }
}
