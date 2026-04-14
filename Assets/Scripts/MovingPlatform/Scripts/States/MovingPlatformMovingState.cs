using UnityEngine;

/// <summary>
/// Platform is moving along its spline toward the knot at TargetKnotIndex.
///
/// Open spline — knot i sits at t = i / (knotCount - 1):
///   PingPong — reverse direction at each end.
///   OneShot  — stop and return to Idle when the last knot is reached.
///
/// Closed spline — knot i sits at t = i / knotCount; t wraps continuously.
///   LoopMode is ignored; the platform loops forever.
/// </summary>
public class MovingPlatformMovingState : MovingPlatformState
{
    public MovingPlatformMovingState(MovingPlatformStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter() { }
    public override void Exit()  { }

    public override void Tick(float tickDelta)
    {
        var brain   = stateMachine.Brain;
        var payload = brain.StatePayload;
        var spline  = brain.SplinePath.Spline;
        int count   = spline.Count;

        float t = payload.SplineT;
        brain.Motor.MoveAlongSpline(brain.SplinePath, ref t, payload.WaypointDirection, brain.PlatformData.Speed, tickDelta);
        payload.SplineT = t;

        float knotT         = KnotNormalizedT(payload.TargetKnotIndex, count, spline.Closed);
        bool  arrivedAtKnot = spline.Closed
            ? HasPassedKnotClosed(payload.SplineT, knotT, payload.WaypointDirection)
            : payload.WaypointDirection > 0 ? t >= knotT : t <= knotT;
        if (!arrivedAtKnot) return;

        // Snap exactly onto the knot so floating-point drift doesn't accumulate
        payload.SplineT = knotT;
        brain.Motor.PlaceOnSpline(brain.SplinePath, knotT);

        if (!TryAdvanceKnot()) return;

        if (brain.PlatformData.WaitDuration > 0f)
            stateMachine.ChangeState(stateMachine.WaitingState);
        // else: stay in MovingState; next Tick moves toward the newly set TargetKnotIndex
    }

    /// <summary>
    /// Advances TargetKnotIndex according to the loop mode or closed-spline wrapping.
    /// Returns false and transitions to Idle when a OneShot route is complete.
    /// </summary>
    private bool TryAdvanceKnot()
    {
        var brain   = stateMachine.Brain;
        var payload = brain.StatePayload;
        var spline  = brain.SplinePath.Spline;
        int count   = spline.Count;
        int next    = payload.TargetKnotIndex + payload.WaypointDirection;

        if (spline.Closed)
        {
            payload.TargetKnotIndex = next % count;
            return true;
        }

        switch (brain.PlatformData.LoopMode)
        {
            case PlatformLoopMode.PingPong:
                if (next >= count)
                {
                    payload.WaypointDirection = -1;
                    next = count - 2;
                }
                else if (next < 0)
                {
                    payload.WaypointDirection = 1;
                    next = 1;
                }
                payload.TargetKnotIndex = Mathf.Clamp(next, 0, count - 1);
                return true;

            default: // OneShot
                if (next >= count || next < 0)
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                    return false;
                }
                payload.TargetKnotIndex = next;
                return true;
        }
    }

    // Closed spline: t wraps, so check within a half-segment window around the knot
    private static bool HasPassedKnotClosed(float t, float knotT, int direction)
    {
        float delta = Mathf.Repeat(t - knotT, 1f);
        return direction > 0 ? delta < 0.5f : delta > 0.5f;
    }

    private static float KnotNormalizedT(int knotIndex, int knotCount, bool closed)
        => knotCount > 0 ? (float)knotIndex / (closed ? knotCount : knotCount - 1) : 0f;
}
