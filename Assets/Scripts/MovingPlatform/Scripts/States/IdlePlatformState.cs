/// <summary>
/// Platform is stationary, waiting for the EventZone to be triggered.
/// Resets direction and target on entry so the platform always starts its
/// route from the beginning when re-triggered after a OneShot cycle.
/// </summary>
public class IdlePlatformState : MovingPlatformState
{
    public IdlePlatformState(MovingPlatformStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        var brain = stateMachine.Brain;
        brain.Motor.ResetVelocity();
        brain.StatePayload.IsActivated       = false;
        brain.StatePayload.WaypointDirection = 1;
        brain.StatePayload.TargetKnotIndex   = 1;
        brain.StatePayload.SplineT           = 0f;
        brain.Motor.PlaceOnSpline(brain.SplinePath, 0f);
    }

    public override void Tick(float tickDelta)
    {
        if (!stateMachine.CurrentInput.IsTriggered) return;

        stateMachine.Brain.StatePayload.IsActivated = true;
        stateMachine.ChangeState(stateMachine.MovingState);
    }

    public override void Exit() { }
}
