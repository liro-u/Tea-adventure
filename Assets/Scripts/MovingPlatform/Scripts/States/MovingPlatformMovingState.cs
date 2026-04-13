/// <summary>
/// Platform is moving toward Waypoints[TargetWaypointIndex].
///
/// On arrival the next target is computed immediately (before entering WaitingState if applicable)
/// so that WaitingState only needs to wait — it does not need to know about waypoint logic.
///
/// Waypoint advancement rules:
///   PingPong — reverse direction at each end, bounce back and forth.
///   Loop     — wrap from the last waypoint back to the first.
///   OneShot  — stop and return to Idle when the last waypoint is reached.
/// </summary>
public class MovingPlatformMovingState : MovingPlatformState
{
    public MovingPlatformMovingState(MovingPlatformStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()  { }
    public override void Exit()   { }

    public override void Tick(float tickDelta)
    {
        var brain  = stateMachine.Brain;
        var target = brain.Waypoints[brain.StatePayload.TargetWaypointIndex].position;

        bool arrived = brain.Motor.MoveTowards(target, brain.PlatformData.Speed, tickDelta);
        if (!arrived) return;

        if (!TryAdvanceWaypoint()) return; // TryAdvanceWaypoint transitions to Idle on OneShot end

        if (brain.PlatformData.WaitDuration > 0f)
            stateMachine.ChangeState(stateMachine.WaitingState);
        // else: stay in MovingState; next Tick moves toward the newly set TargetWaypointIndex
    }

    /// <summary>
    /// Advances TargetWaypointIndex according to the loop mode.
    /// Returns false and transitions to Idle when a OneShot route is complete.
    /// </summary>
    private bool TryAdvanceWaypoint()
    {
        var brain   = stateMachine.Brain;
        var payload = brain.StatePayload;
        int count   = brain.Waypoints.Length;
        int next    = payload.TargetWaypointIndex + payload.WaypointDirection;

        switch (brain.PlatformData.LoopMode)
        {
            case PlatformLoopMode.PingPong:
                if (next >= count)
                {
                    payload.WaypointDirection   = -1;
                    next                        = count - 2;
                }
                else if (next < 0)
                {
                    payload.WaypointDirection   = 1;
                    next                        = 1;
                }
                payload.TargetWaypointIndex = next < 0 ? 0 : next; // guard for single-waypoint edge case
                return true;

            case PlatformLoopMode.Loop:
                payload.TargetWaypointIndex = next % count;
                return true;

            default: // OneShot
                if (next >= count)
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                    return false;
                }
                payload.TargetWaypointIndex = next;
                return true;
        }
    }
}
