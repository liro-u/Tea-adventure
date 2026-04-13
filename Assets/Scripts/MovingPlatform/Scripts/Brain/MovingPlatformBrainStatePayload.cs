public class MovingPlatformBrainStatePayload : IMovingPlatformBrainStatePayload
{
    public int   TargetWaypointIndex { get; set; } = 0;
    public float WaitTimer           { get; set; } = 0f;
    public bool  IsActivated         { get; set; } = false;
    public int   WaypointDirection   { get; set; } = 1;
}
