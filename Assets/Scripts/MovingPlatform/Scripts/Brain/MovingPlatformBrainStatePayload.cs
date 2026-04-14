public class MovingPlatformBrainStatePayload : IMovingPlatformBrainStatePayload
{
    public float SplineT           { get; set; } = 0f;
    public int   TargetKnotIndex   { get; set; } = 1;
    public float WaitTimer         { get; set; } = 0f;
    public bool  IsActivated       { get; set; } = false;
    public int   WaypointDirection { get; set; } = 1;
}
